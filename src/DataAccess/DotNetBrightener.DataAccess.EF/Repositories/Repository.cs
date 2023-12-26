using DotNetBrightener.DataAccess.EF.Events;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.Exceptions;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.DataTransferObjectUtility;
using DotNetBrightener.Plugins.EventPubSub;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DotNetBrightener.DataAccess.EF.Repositories;

public class Repository : IRepository
{
    protected readonly DbContext                    DbContext;
    protected readonly ICurrentLoggedInUserResolver CurrentLoggedInUserResolver;
    protected readonly IEventPublisher              EventPublisher;

    public Repository(DbContext                    dbContext,
                      ICurrentLoggedInUserResolver currentLoggedInUserResolver,
                      IEventPublisher              eventPublisher)
    {
        DbContext                   = dbContext;
        CurrentLoggedInUserResolver = currentLoggedInUserResolver;
        EventPublisher              = eventPublisher;
    }

    public virtual T Get<T>(Expression<Func<T, bool>> expression)
        where T : class
    {
        return Fetch(expression).SingleOrDefault();
    }

    public virtual T GetFirst<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return Fetch(expression).FirstOrDefault();
    }

    public virtual TResult Get<T, TResult>(Expression<Func<T, bool>>    expression,
                                           Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        return Fetch(expression, propertiesPickupExpression).SingleOrDefault();
    }

    public virtual TResult GetFirst<T, TResult>(Expression<Func<T, bool>>    expression,
                                                Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        return Fetch(expression, propertiesPickupExpression).FirstOrDefault();
    }

    public virtual IQueryable<T> Fetch<T>(Expression<Func<T, bool>> expression = null)
        where T : class
    {
        if (expression == null)
            return DbContext.Set<T>().AsQueryable();

        return DbContext.Set<T>().Where(expression);
    }

    public virtual IQueryable<TResult> Fetch<T, TResult>(Expression<Func<T, bool>>    expression,
                                                         Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        if (propertiesPickupExpression == null)
            throw new ArgumentNullException(nameof(propertiesPickupExpression));

        var query = Fetch(expression);

        return query.Select(propertiesPickupExpression);
    }

    public virtual int Count<T>(Expression<Func<T, bool>> expression = null)
        where T : class
    {
        return Fetch(expression).Count();
    }

    public virtual async void Insert<T>(T entity)
        where T : class
    {
        if (entity is BaseEntityWithAuditInfo auditableEntity)
        {
            auditableEntity.CreatedDate = auditableEntity.ModifiedDate = DateTimeOffset.UtcNow;

            if (string.IsNullOrEmpty(auditableEntity.CreatedBy))
                auditableEntity.CreatedBy = CurrentLoggedInUserResolver.CurrentUserName;

            auditableEntity.ModifiedBy = "RECORD_CREATED_EVENT";
        }

        var entityEntry = DbContext.Entry(entity);

        if (entityEntry.State != EntityState.Detached)
        {
            entityEntry.State = EntityState.Added;
        }
        else
        {
            await DbContext.Set<T>().AddAsync(entity);
        }
    }

    public virtual void Insert<T>(IEnumerable<T> entities)
        where T : class
    {
        var entitiesToInserts = entities.Select(_ =>
        {
            if (_ is not BaseEntityWithAuditInfo auditableEntity) return _;

            auditableEntity.CreatedDate =
                auditableEntity.ModifiedDate = DateTimeOffset.UtcNow;

            if (string.IsNullOrEmpty(auditableEntity.CreatedBy))
                auditableEntity.CreatedBy = CurrentLoggedInUserResolver.CurrentUserName;

            auditableEntity.ModifiedBy = "RECORD_CREATED_EVENT";

            return _;
        });

        DbContext.BulkCopy(entitiesToInserts);
    }

    public virtual int CopyRecords<TSource, TTarget>(Expression<Func<TSource, bool>>    conditionExpression,
                                                     Expression<Func<TSource, TTarget>> copyExpression)
        where TSource : class
        where TTarget : class
    {
        var query = Fetch(conditionExpression);

        return query.Insert(DbContext.Set<TTarget>().ToLinqToDBTable(), copyExpression);
    }

    public virtual void Update<T>(T entity) where T : class
    {
        if (entity is BaseEntityWithAuditInfo auditableEntity)
        {
            auditableEntity.ModifiedDate = DateTimeOffset.UtcNow;
            auditableEntity.ModifiedBy   = CurrentLoggedInUserResolver.CurrentUserName;
        }

        var entityEntry = DbContext.Entry(entity);

        if (entityEntry.State == EntityState.Detached)
        {
            DbContext.Set<T>().Attach(entity);
        }

        entityEntry.State = EntityState.Modified;
    }

    public virtual void Update<T>(IEnumerable<T> entities) where T : class
    {
        foreach (var entity in entities)
        {
            Update(entity);
        }
    }

    public virtual int Update<T>(Expression<Func<T, bool>> conditionExpression,
                                 object                    updateExpression,
                                 int?                      expectedAffectedRows = null)
        where T : class
    {
        var finalUpdateExpression = DataTransferObjectUtils.BuildMemberInitExpressionFromDto<T>(updateExpression);

        return Update(conditionExpression, finalUpdateExpression, expectedAffectedRows);
    }

    public virtual int Update<T>(Expression<Func<T, bool>> conditionExpression,
                                 Expression<Func<T, T>>    updateExpression,
                                 int?                      expectedAffectedRows = null)
        where T : class
    {
        var query = DbContext.Set<T>().Where(conditionExpression);

        int updatedRecords;

        int PerformUpdate()
        {
            var updateQueryBuilder = PrepareUpdatePropertiesBuilder(updateExpression);

            return query.ExecutePatchUpdate(updateQueryBuilder);
        }

        if (expectedAffectedRows.HasValue)
        {
            using var dbTransaction = DbContext.Database.BeginTransaction();

            updatedRecords = PerformUpdate();

            if (updatedRecords != expectedAffectedRows.Value)
            {
                dbTransaction.Rollback();

                throw new ExpectedAffectedRecordMismatchException(expectedAffectedRows.Value, updatedRecords);
            }

            dbTransaction.Commit();
        }
        else
        {
            updatedRecords = PerformUpdate();
        }

        return updatedRecords;
    }

    public virtual void DeleteOne<T>(Expression<Func<T, bool>> conditionExpression, bool forceHardDelete = false)
        where T : class
    {
        using var dbTransaction = DbContext.Database.BeginTransaction();

        var affectedRecords = DeleteMany(conditionExpression, forceHardDelete);

        if (affectedRecords != 1)
        {
            dbTransaction.Rollback();

            throw new ExpectedAffectedRecordMismatchException(1, affectedRecords);
        }

        dbTransaction.Commit();
    }

    public virtual int DeleteMany<T>(Expression<Func<T, bool>> conditionExpression,
                                     bool                      forceHardDelete = false)
        where T : class
    {
        const string isDeletedFieldName = nameof(BaseEntityWithAuditInfo.IsDeleted);

        forceHardDelete =
            forceHardDelete || !typeof(T).HasProperty<bool>(isDeletedFieldName);

        var updatedRecords = forceHardDelete
                                 ? Fetch(conditionExpression).ExecuteDelete()
                                 : Update(conditionExpression,
                                          new
                                          {
                                              IsDeleted = true
                                          });

        return updatedRecords;
    }

    public int CommitChanges()
    {
        EntityEntry[] insertedEntities = Array.Empty<EntityEntry>();
        EntityEntry[] updatedEntities  = Array.Empty<EntityEntry>();

        try
        {
            if (!DbContext.ChangeTracker.HasChanges())
            {
                return 0;
            }

            OnBeforeSaveChanges(out insertedEntities, out updatedEntities);

            return DbContext.SaveChanges();
        }
        catch (ObjectDisposedException)
        {
            return 0;
        }
        finally
        {
            if (insertedEntities.Length != 0 &&
                updatedEntities.Length != 0)
                OnAfterSaveChanges(insertedEntities, updatedEntities);
        }
    }


    public void OnBeforeSaveChanges(out EntityEntry[] insertedEntities,
                                    out EntityEntry[] updatedEntities)
    {
        if (!DbContext.ChangeTracker.HasChanges())
        {
            insertedEntities = Array.Empty<EntityEntry>();
            updatedEntities  = Array.Empty<EntityEntry>();

            return;
        }

        EntityEntry[] entityEntries = DbContext.ChangeTracker
                                               .Entries()
                                               .ToArray();

        insertedEntities = entityEntries.Where(e => e.State == EntityState.Added)
                                        .ToArray();

        updatedEntities = entityEntries.Where(e => e.State == EntityState.Modified ||
                                                   e.State == EntityState.Deleted)
                                       .ToArray();

        EventPublisher.Publish(new DbContextBeforeSaveChanges
                       {
                           InsertedEntityEntries = insertedEntities,
                           UpdatedEntityEntries  = updatedEntities,
                           CurrentUserId         = CurrentLoggedInUserResolver.CurrentUserId,
                           CurrentUserName       = CurrentLoggedInUserResolver.CurrentUserName,
                       })
                      .Wait();
    }

    public void OnAfterSaveChanges(EntityEntry[] insertedEntities, EntityEntry[] updatedEntities)
    {
        if (insertedEntities.Length == 0 &&
            updatedEntities.Length == 0)
            return;

        EventPublisher.Publish(new DbContextAfterSaveChanges
        {
            InsertedEntityEntries = insertedEntities,
            UpdatedEntityEntries  = updatedEntities,
            CurrentUserId         = CurrentLoggedInUserResolver.CurrentUserId,
            CurrentUserName       = CurrentLoggedInUserResolver.CurrentUserName,
        });
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private SetPropertyBuilder<T> PrepareUpdatePropertiesBuilder<T>(Expression<Func<T, T>> updateExpression)
    {
        if (updateExpression.Body is not MemberInitExpression memberInitExpression)
            throw new InvalidOperationException("Invalid expression type for updating entity");

        var memberAssignmentList = new List<MemberBinding>(memberInitExpression.Bindings);

        var setPropBuilder = new SetPropertyBuilder<T>();

        bool hasModifiedDate = false, hasModifiedBy = false;

        foreach (MemberAssignment binding in memberAssignmentList.OfType<MemberAssignment>())
        {
            var propertyInfo = (PropertyInfo)binding.Member;

            if (propertyInfo.Name == nameof(BaseEntityWithAuditInfo.ModifiedDate) &&
                propertyInfo.PropertyType == typeof(DateTimeOffset?))
            {
                hasModifiedDate = true;
            }

            if (propertyInfo.Name == nameof(BaseEntityWithAuditInfo.ModifiedBy) &&
                propertyInfo.PropertyType == typeof(string))
            {
                hasModifiedBy = true;
            }

            var setPropMethod = typeof(SetPropertyBuilder<T>)
               .GetMethod(nameof(SetPropertyBuilder<T>.SetPropertyByNameAndExpression));


            setPropMethod?.MakeGenericMethod(propertyInfo.PropertyType)
                          .Invoke(setPropBuilder,
                                  new object[]
                                  {
                                      propertyInfo.Name, Expression.Lambda(binding.Expression, updateExpression.Parameters)
                                  });
        }

        if (!hasModifiedDate &&
            typeof(T).IsAssignableTo(typeof(BaseEntityWithAuditInfo)))
        {
            setPropBuilder.SetPropertyByName<DateTimeOffset?>(nameof(BaseEntityWithAuditInfo.ModifiedDate),
                                                              DateTimeOffset.UtcNow);
        }

        if (!hasModifiedBy &&
            typeof(T).HasProperty<string>(nameof(BaseEntityWithAuditInfo.ModifiedBy)))
        {
            setPropBuilder.SetPropertyByName(nameof(BaseEntityWithAuditInfo.ModifiedBy),
                                             CurrentLoggedInUserResolver.CurrentUserName);
        }

        return setPropBuilder;
    }
}
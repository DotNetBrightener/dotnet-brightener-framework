using DotNetBrightener.DataAccess.EF.Events;
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
using System.Linq;
using System.Linq.Expressions;

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

    public virtual TResult Get<T, TResult>(Expression<Func<T, bool>>    expression,
                                           Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        return Fetch(expression, propertiesPickupExpression).SingleOrDefault();
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
        var entitiesToInserts = !typeof(BaseEntityWithAuditInfo).IsAssignableFrom(typeof(T))
                                    ? entities
                                    : entities.Select(_ =>
                                    {
                                        if (_ is BaseEntityWithAuditInfo auditableEntity)
                                        {
                                            auditableEntity.CreatedDate =
                                                auditableEntity.ModifiedDate = DateTimeOffset.UtcNow;

                                            if (string.IsNullOrEmpty(auditableEntity.CreatedBy))
                                                auditableEntity.CreatedBy = CurrentLoggedInUserResolver.CurrentUserName;

                                            auditableEntity.ModifiedBy = "RECORD_CREATED_EVENT";
                                        }

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

        var finalUpdateExpression = AppendAuditInfoToExpression(updateExpression);

        int PerformUpdate()
        {
            return query.Update(finalUpdateExpression);
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
        forceHardDelete =
            forceHardDelete || !typeof(T).HasProperty<bool>(nameof(BaseEntityWithAuditInfo.IsDeleted));

        var query = Fetch(conditionExpression);
        int updatedRecords;

        if (forceHardDelete)
        {
            updatedRecords = query.Delete();
        }
        else
        {
            updatedRecords = Update(conditionExpression,
                                    new
                                    {
                                        IsDeleted = true,
                                        Deleted   = DateTimeOffset.UtcNow
                                    });
        }

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

    private Expression<Func<T, T>> AppendAuditInfoToExpression<T>(Expression<Func<T, T>> updateExpression)
        where T : class
    {

        if (!typeof(BaseEntityWithAuditInfo).IsAssignableFrom(typeof(T)) ||
            updateExpression.Body is not MemberInitExpression memberInitExpression)
            return updateExpression;

        var memberAssignmentList = new List<MemberBinding>(memberInitExpression.Bindings);

        var destinationType = typeof(T);

        if (memberAssignmentList.All(_ => _.Member.Name != nameof(BaseEntityWithAuditInfo.ModifiedBy)))
        {
            // assign value to field LastUpdateBy
            var updatedByUserFieldName = nameof(BaseEntityWithAuditInfo.ModifiedBy);
            var updatedByUserField     = destinationType.GetProperty(updatedByUserFieldName);

            if (updatedByUserField != null)
            {
                memberAssignmentList.Add(Expression.Bind(
                                                         updatedByUserField,
                                                         Expression.Constant(CurrentLoggedInUserResolver
                                                                                .CurrentUserName,
                                                                             updatedByUserField.PropertyType)
                                                        )
                                        );
            }
        }

        if (memberAssignmentList.All(_ => _.Member.Name != nameof(BaseEntityWithAuditInfo.ModifiedDate)))
        {
            // assign value to field LastUpdate
            var lastUpdateFieldName = nameof(BaseEntityWithAuditInfo.ModifiedDate);

            var updatedDateField = destinationType.GetProperty(lastUpdateFieldName);

            if (updatedDateField != null)
            {
                memberAssignmentList.Add(Expression.Bind(
                                                         updatedDateField,
                                                         Expression.Constant(DateTimeOffset.UtcNow,
                                                                             updatedDateField.PropertyType)
                                                        )
                                        );
            }
        }

        memberInitExpression = memberInitExpression.Update(memberInitExpression.NewExpression, memberAssignmentList);

        updateExpression = updateExpression.Update(memberInitExpression, updateExpression.Parameters);

        return updateExpression;
    }
}
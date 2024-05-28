#nullable enable

using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.Events;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.Exceptions;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.DataAccess.Utils;
using DotNetBrightener.Plugins.EventPubSub;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;
using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.DataAccess.Models.Guards;

namespace DotNetBrightener.DataAccess.EF.Repositories;

public class Repository : IRepository
{
    protected readonly DbContext                     DbContext;
    protected readonly ICurrentLoggedInUserResolver? CurrentLoggedInUserResolver;
    protected readonly IEventPublisher?              EventPublisher;
    protected readonly IDateTimeProvider?            DateTimeProvider;
    protected          ILogger                       Logger { get; init; }

    public Repository(DbContext        dbContext,
                      IServiceProvider serviceProvider,
                      ILoggerFactory   loggerFactory)
    {
        DbContext                   = dbContext;
        CurrentLoggedInUserResolver = serviceProvider.TryGet<ICurrentLoggedInUserResolver>();
        EventPublisher              = serviceProvider.TryGet<IEventPublisher>();
        DateTimeProvider            = serviceProvider.TryGet<IDateTimeProvider>();
        Logger                      = loggerFactory.CreateLogger(GetType());
    }

    public virtual T? Get<T>(Expression<Func<T, bool>> expression)
        where T : class
    {
        return Fetch(expression).SingleOrDefault();
    }

    public virtual T? GetFirst<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return Fetch(expression).FirstOrDefault();
    }

    public virtual TResult? Get<T, TResult>(Expression<Func<T, bool>>?   expression,
                                            Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        return Fetch(expression, propertiesPickupExpression).SingleOrDefault();
    }

    public virtual TResult? GetFirst<T, TResult>(Expression<Func<T, bool>>?   expression,
                                                 Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        return Fetch(expression, propertiesPickupExpression).FirstOrDefault();
    }

    public virtual IQueryable<T> Fetch<T>(Expression<Func<T, bool>>? expression = null)
        where T : class
    {
        if (expression == null)
            return DbContext.Set<T>().AsQueryable();

        return DbContext.Set<T>().Where(expression);
    }

    public virtual IQueryable<T> FetchHistory<T>(Expression<Func<T, bool>>? expression,
                                                 DateTimeOffset?            from,
                                                 DateTimeOffset?            to)
        where T : class, new()
    {
        if (!typeof(T).HasAttribute<HistoryEnabledAttribute>())
        {
            throw new VersioningNotSupportedException<T>();
        }

        var initialQuery = DbContext.Set<T>();

        var temporalQuery = initialQuery.TemporalAll();

        if (from is not null &&
            to is not null)
        {
            temporalQuery = initialQuery.TemporalFromTo(from.Value.UtcDateTime, to.Value.UtcDateTime)
                                        .OrderBy(entry =>
                                                     Microsoft.EntityFrameworkCore.EF.Property<DateTime>(entry,
                                                                                                         "PeriodStart"));
        }

        if (expression is not null)
        {
            temporalQuery = temporalQuery.Where(expression);
        }

        return temporalQuery;
    }

    public virtual IQueryable<TResult> Fetch<T, TResult>(Expression<Func<T, bool>>?   expression,
                                                         Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        if (propertiesPickupExpression == null)
            throw new ArgumentNullException(nameof(propertiesPickupExpression));

        var query = Fetch(expression);

        return query.Select(propertiesPickupExpression);
    }

    public virtual int Count<T>(Expression<Func<T, bool>>? expression = null)
        where T : class
    {
        return Fetch(expression).Count();
    }

    public virtual void Insert<T>(T entity)
        where T : class
    {
        if (entity is IAuditableEntity auditableEntity)
        {
            auditableEntity.CreatedDate = DateTimeProvider?.UtcNow ?? DateTime.UtcNow;

            if (string.IsNullOrEmpty(auditableEntity.CreatedBy))
                auditableEntity.CreatedBy = CurrentLoggedInUserResolver?.CurrentUserName;
        }

        EventPublisher?.Publish(new EntityCreating<T>
                        {
                            UserName = CurrentLoggedInUserResolver?.CurrentUserName,
                            UserId   = CurrentLoggedInUserResolver?.CurrentUserId,
                            Entity   = entity
                        })
                       .Wait();

        var entityEntry = DbContext.Entry(entity);

        if (entityEntry.State != EntityState.Detached)
        {
            entityEntry.State = EntityState.Added;
        }
        else
        {
            DbContext.Set<T>().Add(entity);
        }
    }

    public virtual void InsertMany<T>(IEnumerable<T> entities)
        where T : class
    {
        var now = DateTimeProvider?.UtcNow ?? DateTime.UtcNow;

        T TransformExpression(T entity)
        {
            if (entity is not IAuditableEntity auditableEntity) return entity;

            auditableEntity.CreatedDate = now;

            if (string.IsNullOrEmpty(auditableEntity.CreatedBy))
                auditableEntity.CreatedBy = CurrentLoggedInUserResolver?.CurrentUserName;

            EventPublisher?.Publish(new EntityCreating<T>
                            {
                                UserName = CurrentLoggedInUserResolver?.CurrentUserName,
                                UserId   = CurrentLoggedInUserResolver?.CurrentUserId,
                                Entity   = entity
                            })
                           .Wait();

            return entity;
        }

        var entitiesToInserts = entities.Select(TransformExpression)
                                        .ToList();

        try
        {
            Logger.LogInformation("BulkInserting {records} records of type {entityType}...",
                                  entitiesToInserts.Count,
                                  typeof(T).Name);

            DbContext.BulkCopy(entitiesToInserts);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e,
                              "BulkInsert failed to insert {numberOfRecords} records entities of type {Type}. Retrying with slow insert...",
                              entitiesToInserts.Count,
                              typeof(T).Name);

            DbContext.Set<T>().AddRange(entitiesToInserts);
        }
    }

    public virtual int CopyRecords<TSource, TTarget>(Expression<Func<TSource, bool>>?   conditionExpression,
                                                     Expression<Func<TSource, TTarget>> copyExpression)
        where TSource : class
        where TTarget : class
    {
        var query = Fetch(conditionExpression);

        return query.Insert(DbContext.Set<TTarget>().ToLinqToDBTable(), copyExpression);
    }

    public virtual void Update<T>(T entity) where T : class
    {
        EventPublisher?.Publish(new EntityUpdating<T>()
                        {
                            UserName = CurrentLoggedInUserResolver?.CurrentUserName,
                            UserId   = CurrentLoggedInUserResolver?.CurrentUserId,
                            Entity   = entity
                        })
                       .Wait();

        if (entity is BaseEntityWithAuditInfo auditableEntity)
        {
            auditableEntity.ModifiedDate = DateTimeProvider?.UtcNow ?? DateTime.UtcNow;
            auditableEntity.ModifiedBy   = CurrentLoggedInUserResolver?.CurrentUserName;
        }

        var entityEntry = DbContext.Entry(entity);

        if (entityEntry.State == EntityState.Detached)
        {
            DbContext.Set<T>().Attach(entity);
        }

        entityEntry.State = EntityState.Modified;
    }

    public virtual void Update<T>(T entity, object dataToUpdate) where T : class
    {
        var ignoreProperties = typeof(T).GetPropertiesWithNoClientSideUpdate();

        entity.UpdateFromDto(dataToUpdate,
                             out var auditTrail,
                             ignoreProperties);

        Update(entity);
    }

    public virtual void UpdateMany<T>(IEnumerable<T> entities) where T : class => UpdateMany(entities.ToArray());

    public virtual void UpdateMany<T>(params T[] entities) where T : class
    {
        foreach (var entity in entities)
        {
            Update(entity);
        }
    }

    public virtual int Update<T>(Expression<Func<T, bool>>? conditionExpression,
                                 object                     updateExpression,
                                 int?                       expectedAffectedRows = null)
        where T : class
    {
        var finalUpdateExpression = DataTransferObjectUtils.BuildMemberInitExpressionFromDto<T>(updateExpression);

        return Update(conditionExpression, finalUpdateExpression, expectedAffectedRows);
    }

    public virtual int Update<T>(Expression<Func<T, bool>>? conditionExpression,
                                 Expression<Func<T, T>>     updateExpression,
                                 int?                       expectedAffectedRows = null)
        where T : class
    {
        var query = conditionExpression is not null
                        ? DbContext.Set<T>().Where(conditionExpression)
                        : DbContext.Set<T>();

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

        EventPublisher?.Publish(new EntityUpdatedByExpression<T>
        {
            FilterExpression = conditionExpression,
            UpdateExpression = updateExpression,
            AffectedRecords  = updatedRecords,
            UserName         = CurrentLoggedInUserResolver?.CurrentUserName,
            UserId           = CurrentLoggedInUserResolver?.CurrentUserId
        });

        return updatedRecords;
    }

    public virtual void DeleteOne<T>(Expression<Func<T, bool>>? conditionExpression,
                                     string?                    reason          = null,
                                     bool                       forceHardDelete = false)
        where T : class
    {
        using (var dbTransaction = DbContext.Database.BeginTransaction())
        {
            var affectedRecords = DeleteMany(conditionExpression, reason, forceHardDelete);

            if (affectedRecords != 1)
            {
                dbTransaction.Rollback();

                throw new ExpectedAffectedRecordMismatchException(1, affectedRecords);
            }

            dbTransaction.Commit();
        }
    }

    public virtual int DeleteMany<T>(Expression<Func<T, bool>>? conditionExpression,
                                     string?                    reason          = null,
                                     bool                       forceHardDelete = false)
        where T : class
    {
        const string isDeletedFieldName = nameof(IAuditableEntity.IsDeleted);

        forceHardDelete =
            forceHardDelete || !typeof(T).HasProperty<bool>(isDeletedFieldName);

        var updatedRecords = forceHardDelete
                                 ? Fetch(conditionExpression).ExecuteDelete()
                                 : Update(conditionExpression,
                                          new
                                          {
                                              IsDeleted      = true,
                                              DeletedDate    = DateTimeOffset.UtcNow,
                                              DeletedBy      = CurrentLoggedInUserResolver?.CurrentUserName,
                                              DeletionReason = reason
                                          },
                                          expectedAffectedRows: null);


        EventPublisher?.Publish(new EntityDeletedByExpression<T>
        {
            DeletionReason   = reason,
            FilterExpression = conditionExpression,
            AffectedRecords  = updatedRecords,
            UserName         = CurrentLoggedInUserResolver?.CurrentUserName,
            UserId           = CurrentLoggedInUserResolver?.CurrentUserId
        });

        return updatedRecords;
    }

    public void RestoreOne<T>(Expression<Func<T, bool>>? conditionExpression) where T : class
    {
        Guards.AssertEntityRecoverable<T>();

        using var dbTransaction = DbContext.Database.BeginTransaction();

        var affectedRecords = RestoreMany(conditionExpression);

        if (affectedRecords != 1)
        {
            dbTransaction.Rollback();

            throw new ExpectedAffectedRecordMismatchException(1, affectedRecords);
        }

        dbTransaction.Rollback();
    }

    public virtual int RestoreMany<T>(Expression<Func<T, bool>>? conditionExpression)
        where T : class
    {
        Guards.AssertEntityRecoverable<T>();

        var updatedRecords = Update(conditionExpression,
                                    new
                                    {
                                        IsDeleted      = false,
                                        DeletedDate    = default(DateTimeOffset?),
                                        DeletedBy      = default(string),
                                        DeletionReason = default(string)
                                    },
                                    expectedAffectedRows: null);

        return updatedRecords;
    }

    public int CommitChanges()
    {
        EntityEntry[] insertedEntities = [];
        EntityEntry[] updatedEntities  = [];

        try
        {
            if (!DbContext.ChangeTracker.HasChanges())
            {
                return 0;
            }

            OnBeforeSaveChanges(out insertedEntities, out updatedEntities);

            return DbContext.SaveChanges();
        }
        catch (ObjectDisposedException ex)
        {
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("No database provider has been configured for this DbContext"))
            {
                Logger.LogWarning("Error while configuring the DbContext of type {DbContextType}",
                                  DbContext.GetType().FullName);
            }

            throw;
        }
        finally
        {
            if (insertedEntities.Length != 0 ||
                updatedEntities.Length != 0)
                OnAfterSaveChanges(insertedEntities, updatedEntities);
        }
    }


    public void OnBeforeSaveChanges(out EntityEntry[] insertedEntities,
                                    out EntityEntry[] updatedEntities)
    {
        if (!DbContext.ChangeTracker.HasChanges())
        {
            insertedEntities = [];
            updatedEntities  = [];

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

        EventPublisher?.Publish(new DbContextBeforeSaveChanges
                        {
                            InsertedEntityEntries = insertedEntities,
                            UpdatedEntityEntries  = updatedEntities,
                            CurrentUserId         = CurrentLoggedInUserResolver?.CurrentUserId,
                            CurrentUserName       = CurrentLoggedInUserResolver?.CurrentUserName,
                        })
                       .Wait();
    }

    public void OnAfterSaveChanges(EntityEntry[] insertedEntities, EntityEntry[] updatedEntities)
    {
        if (insertedEntities.Length == 0 &&
            updatedEntities.Length == 0)
        {
            return;
        }

        EventPublisher?.Publish(new DbContextAfterSaveChanges
        {
            InsertedEntityEntries = insertedEntities,
            UpdatedEntityEntries  = updatedEntities,
            CurrentUserId         = CurrentLoggedInUserResolver?.CurrentUserId,
            CurrentUserName       = CurrentLoggedInUserResolver?.CurrentUserName,
        });
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
                           [
                               propertyInfo.Name,
                                      Expression.Lambda(binding.Expression, updateExpression.Parameters)
                           ]);
        }

        if (!hasModifiedDate &&
            typeof(T).IsAssignableTo(typeof(IAuditableEntity)))
        {
            setPropBuilder.SetPropertyByName<DateTimeOffset?>(nameof(IAuditableEntity.ModifiedDate),
                                                              DateTimeProvider?.UtcNow ?? DateTime.UtcNow);
        }

        if (!hasModifiedBy &&
            typeof(T).HasProperty<string>(nameof(IAuditableEntity.ModifiedBy)))
        {
            setPropBuilder.SetPropertyByName(nameof(IAuditableEntity.ModifiedBy),
                                             CurrentLoggedInUserResolver?.CurrentUserName);
        }

        return setPropBuilder;
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}
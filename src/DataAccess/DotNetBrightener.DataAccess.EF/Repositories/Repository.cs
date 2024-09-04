#nullable enable

using System.Data;
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Auditing;
using DotNetBrightener.DataAccess.EF.Events;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.DataAccess.Exceptions;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Models.Guards;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.DataAccess.Utils;
using DotNetBrightener.Plugins.EventPubSub;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Common;

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
        where T : class => GetAsync(expression).Result;

    public virtual async Task<T?> GetAsync<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return await Fetch(expression).SingleOrDefaultAsync();
    }

    public virtual T? GetFirst<T>(Expression<Func<T, bool>> expression)
        where T : class => GetFirstAsync(expression).Result;

    public virtual async Task<T?> GetFirstAsync<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return await Fetch(expression).FirstOrDefaultAsync();
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

        if (from is not null ||
            to is not null)
        {
            from ??= DateTimeOffset.UnixEpoch;

            to ??= DateTimeOffset.UtcNow;

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
        where T : class => CountAsync(expression).Result;

    public virtual async Task<int> CountAsync<T>(Expression<Func<T, bool>>? expression = null)
        where T : class
    {
        return expression is null
                   ? await DbContext.Set<T>().CountAsync()
                   : await DbContext.Set<T>().CountAsync(expression);
    }

    public virtual async Task<int> CountNonDeletedAsync<T>(Expression<Func<T, bool>>? expression = null) where T : class
    {
        if (!typeof(T).HasProperty<bool>(nameof(IAuditableEntity.IsDeleted)))
        {
            throw new InvalidOperationException($"Entity of type {typeof(T).Name} does not have soft-delete capability");
        }

        var query = DbContext.Set<T>().Where($"{nameof(IAuditableEntity.IsDeleted)} != True");

        return expression is null
                   ? await query.CountAsync()
                   : await query.CountAsync(expression);
    }

    public bool Any<T>(Expression<Func<T, bool>>? expression = null)
        where T : class => AnyAsync(expression).Result;

    public virtual async Task<bool> AnyAsync<T>(Expression<Func<T, bool>>? expression = null) 
        where T : class
    {
        return expression is null
                   ? await DbContext.Set<T>().AnyAsync()
                   : await DbContext.Set<T>().AnyAsync(expression);
    }

    public virtual void Insert<T>(T entity)
        where T : class => InsertAsync(entity).Wait();

    public virtual async Task InsertAsync<T>(T entity)
        where T : class
    {
        if (entity is IAuditableEntity auditableEntity)
        {
            if (auditableEntity.CreatedDate is null)
                auditableEntity.CreatedDate = DateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow;

            if (string.IsNullOrEmpty(auditableEntity.CreatedBy))
                auditableEntity.CreatedBy = CurrentLoggedInUserResolver?.CurrentUserName;
        }

        if (EventPublisher is not null)
        {
            await EventPublisher.Publish(new EntityCreating<T>
            {
                UserName = CurrentLoggedInUserResolver?.CurrentUserName,
                UserId   = CurrentLoggedInUserResolver?.CurrentUserId,
                Entity   = entity
            });
        }

        var entityEntry = DbContext.Entry(entity);

        if (entityEntry.State != EntityState.Detached)
        {
            entityEntry.State = EntityState.Added;
            DbContext.Set<T>()
                     .Attach(entity);
        }
        else
        {
            await DbContext.Set<T>()
                           .AddAsync(entity);
        }
    }

    public virtual void InsertMany<T>(IEnumerable<T> entities)
        where T : class => InsertManyAsync(entities).Wait();

    public virtual async Task InsertManyAsync<T>(IEnumerable<T> entities) 
        where T : class
    {
        var entitiesToInserts = entities.Select(TransformExpression)
                                        .ToList();

        if (EventPublisher is not null)
        {
            await entitiesToInserts.ParallelForEachAsync(async entity =>
            {
                await EventPublisher.Publish(new EntityCreating<T>
                {
                    UserName = CurrentLoggedInUserResolver?.CurrentUserName,
                    UserId   = CurrentLoggedInUserResolver?.CurrentUserId,
                    Entity   = entity
                });
            });
        }

        await DbContext.Set<T>()
                       .AddRangeAsync(entitiesToInserts);
    }

    public virtual void BulkInsert<T>(IEnumerable<T> entities)
        where T : class => BulkInsertAsync(entities).Wait();

    public virtual async Task BulkInsertAsync<T>(IEnumerable<T> entities)
        where T : class
    {

        var entitiesToInserts = entities.Select(TransformExpression)
                                        .ToList();

        if (EventPublisher is not null)
        {
            await entitiesToInserts.ParallelForEachAsync(async entity =>
            {
                await EventPublisher.Publish(new EntityCreating<T>
                {
                    UserName = CurrentLoggedInUserResolver?.CurrentUserName,
                    UserId   = CurrentLoggedInUserResolver?.CurrentUserId,
                    Entity   = entity
                });
            });
        }

        try
        {
            Logger.LogInformation("BulkInserting {records} records of type {entityType}...",
                                  entitiesToInserts.Count,
                                  typeof(T).Name);

            await DbContext.BulkCopyAsync(entitiesToInserts);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e,
                              "BulkInsert failed to insert {numberOfRecords} records entities of type {Type}. Retrying with slow insert...",
                              entitiesToInserts.Count,
                              typeof(T).Name);

            await DbContext.Set<T>()
                           .AddRangeAsync(entitiesToInserts);
        }
    }

    private T TransformExpression<T>(T entity)
    {
        if (entity is not IAuditableEntity auditableEntity) return entity;

        if (auditableEntity.CreatedDate is null)
            auditableEntity.CreatedDate = DateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow;

        if (string.IsNullOrEmpty(auditableEntity.CreatedBy))
            auditableEntity.CreatedBy = CurrentLoggedInUserResolver?.CurrentUserName;

        return entity;
    }

    public virtual int CopyRecords<TSource, TTarget>(Expression<Func<TSource, bool>>?   conditionExpression,
                                                     Expression<Func<TSource, TTarget>> copyExpression)
        where TSource : class
        where TTarget : class => CopyRecordsAsync(conditionExpression, copyExpression).Result;

    public virtual async Task<int> CopyRecordsAsync<TSource, TTarget>(
        Expression<Func<TSource, bool>>?   conditionExpression,
        Expression<Func<TSource, TTarget>> copyExpression)
        where TSource : class
        where TTarget : class
    {
        var query = Fetch(conditionExpression);

        return await LinqToDB.LinqExtensions.InsertAsync(query, DbContext.Set<TTarget>().ToLinqToDBTable(), copyExpression);
    }

    public virtual void Update<T>(T entity) where T : class => UpdateAsync(entity).Wait();

    public async Task UpdateAsync<T>(T entity) where T : class
    {
        await UpdateWithAuditTrail(entity, auditTrail: null);
    }

    public virtual void Update<T>(T entity, object dataToUpdate, params string[] propertiesToIgnoreUpdate)
        where T : class
        => UpdateAsync(entity, dataToUpdate, propertiesToIgnoreUpdate).Wait();

    public virtual async Task UpdateAsync<T>(T entity, object dataToUpdate, params string[] propertiesToIgnoreUpdate)
        where T : class
    {
        var definedIgnoreProperties = typeof(T).GetPropertiesWithNoClientSideUpdate();

        var propertiesToIgnore = definedIgnoreProperties.Concat(propertiesToIgnoreUpdate)
                                                        .Distinct()
                                                        .ToArray();

        entity.UpdateFromDto(dataToUpdate,
                             out var auditTrail,
                             propertiesToIgnore);

        await UpdateWithAuditTrail(entity, auditTrail);
    }

    protected virtual async Task UpdateWithAuditTrail<T>(T entity, AuditTrail<T>? auditTrail) where T : class
    {
        if (EventPublisher is not null)
        {
            await EventPublisher.Publish(new EntityUpdating<T>()
            {
                UserName   = CurrentLoggedInUserResolver?.CurrentUserName,
                UserId     = CurrentLoggedInUserResolver?.CurrentUserId,
                Entity     = entity,
                AuditTrail = auditTrail
            });
        }

        if (entity is BaseEntityWithAuditInfo auditableEntity)
        {
            auditableEntity.ModifiedDate = DateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow;
            if (string.IsNullOrWhiteSpace(auditableEntity.ModifiedBy))
                auditableEntity.ModifiedBy = CurrentLoggedInUserResolver?.CurrentUserName;
        }

        var entityEntry = DbContext.Entry(entity);

        if (entityEntry.State == EntityState.Detached)
        {
            DbContext.Set<T>().Attach(entity);
        }

        entityEntry.State = EntityState.Modified;
    }

    public virtual void UpdateMany<T>(IEnumerable<T> entities) where T : class => UpdateMany(entities.ToArray());

    public virtual void UpdateMany<T>(params T[] entities) where T : class
        => UpdateManyAsync(entities).Wait();

    public virtual async Task UpdateManyAsync<T>(params T[] entities) where T : class
    {
        await entities.ParallelForEachAsync(UpdateAsync);
    }

    public virtual int Update<T>(Expression<Func<T, bool>>? conditionExpression,
                                 object                     updateExpression,
                                 int?                       expectedAffectedRows = null)
        where T : class => UpdateAsync(conditionExpression, updateExpression, expectedAffectedRows).Result;

    public virtual async Task<int> UpdateAsync<T>(Expression<Func<T, bool>>? conditionExpression,
                                                  object                     updateExpression,
                                                  int?                       expectedAffectedRows = null)
        where T : class
    {
        var finalUpdateExpression = DataTransferObjectUtils.BuildMemberInitExpressionFromDto<T>(updateExpression);

        return await UpdateAsync(conditionExpression, finalUpdateExpression, expectedAffectedRows);
    }

    public virtual int Update<T>(Expression<Func<T, bool>>? conditionExpression,
                                 Expression<Func<T, T>>     updateExpression,
                                 int?                       expectedAffectedRows = null)
        where T : class => UpdateAsync(conditionExpression, updateExpression, expectedAffectedRows).Result;

    public virtual async Task<int> UpdateAsync<T>(Expression<Func<T, bool>>? conditionExpression,
                                                  Expression<Func<T, T>>     updateExpression,
                                                  int?                       expectedAffectedRows = null)
        where T : class
    {
        var query = conditionExpression is not null
                        ? DbContext.Set<T>().Where(conditionExpression)
                        : DbContext.Set<T>();

        int updatedRecords = 0;

        if (expectedAffectedRows.HasValue)
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();

            async Task ExecuteUpdate()
            {
                updatedRecords = await PerformUpdate(query, updateExpression);

                if (updatedRecords != expectedAffectedRows.Value)
                {
                    throw new ExpectedAffectedRecordMismatchException(expectedAffectedRows.Value,
                                                                      updatedRecords);
                }
            }

            await executionStrategy.ExecuteInTransactionAsync(ExecuteUpdate,
                                                              async () => updatedRecords == expectedAffectedRows.Value);
        }
        else
        {
            updatedRecords = await PerformUpdate(query, updateExpression);
        }

        if (EventPublisher != null)
        {
            _ = EventPublisher.Publish(new EntityUpdatedByExpression<T>
            {
                FilterExpression = conditionExpression,
                UpdateExpression = updateExpression,
                AffectedRecords  = updatedRecords,
                UserName         = CurrentLoggedInUserResolver?.CurrentUserName,
                UserId           = CurrentLoggedInUserResolver?.CurrentUserId
            });
        }

        return updatedRecords;
    }

    public void DeleteOne<T>(T entity, string? reason = null, bool forceHardDelete = false) where T : class
        => DeleteOneAsync(entity, reason, forceHardDelete).Wait();

    public virtual async Task DeleteOneAsync<T>(T entity, string? reason = null, bool forceHardDelete = false)
        where T : class
    {
        var entityDeletingEvent = new EntityDeleting<T>()
        {
            UserName       = CurrentLoggedInUserResolver?.CurrentUserName,
            UserId         = CurrentLoggedInUserResolver?.CurrentUserId,
            Entity         = entity,
            DeletionReason = reason
        };

        if (EventPublisher is not null)
        {
            await EventPublisher.Publish(entityDeletingEvent);
        }

        const string isDeletedFieldName = nameof(IAuditableEntity.IsDeleted);

        forceHardDelete =
            forceHardDelete || !typeof(T).HasProperty<bool>(isDeletedFieldName);


        if (!forceHardDelete &&
            entity is BaseEntityWithAuditInfo auditableEntity)
        {
            auditableEntity.IsDeleted      = true;
            auditableEntity.DeletedDate    = DateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow;
            auditableEntity.DeletedBy      = CurrentLoggedInUserResolver?.CurrentUserName;
            auditableEntity.DeletionReason = entityDeletingEvent.DeletionReason;

            var entityEntry = DbContext.Entry(entity);

            if (entityEntry.State == EntityState.Detached)
            {
                DbContext.Set<T>().Attach(entity);
            }

            entityEntry.State = EntityState.Modified;
        }
        else
        {
            DbContext.Set<T>().Remove(entity);
        }
    }

    private async Task<int> PerformUpdate<T>(IQueryable<T> query, Expression<Func<T, T>> updateExpression)
        where T : class
    {
        var updateQueryBuilder = PrepareUpdatePropertiesBuilder(updateExpression);

        return await query.ExecutePatchUpdateAsync(updateQueryBuilder);
    }

    public virtual void DeleteOne<T>(Expression<Func<T, bool>>? conditionExpression,
                                     string?                    reason          = null,
                                     bool                       forceHardDelete = false)
        where T : class
        => DeleteOneAsync(conditionExpression, reason, forceHardDelete).Wait();

    public virtual async Task DeleteOneAsync<T>(Expression<Func<T, bool>>? conditionExpression,
                                                string?                    reason          = null,
                                                bool                       forceHardDelete = false)
        where T : class
    {
        const string isDeletedFieldName = nameof(IAuditableEntity.IsDeleted);

        forceHardDelete =
            forceHardDelete || !typeof(T).HasProperty<bool>(isDeletedFieldName);

        if (forceHardDelete)
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();

            int updatedRecords = 0;

            async Task ExecuteDelete()
            {
                updatedRecords = await Fetch(conditionExpression).ExecuteDeleteAsync();

                if (updatedRecords != 1)
                {
                    throw new ExpectedAffectedRecordMismatchException(1,
                                                                      updatedRecords);
                }
            }

            await executionStrategy.ExecuteInTransactionAsync(ExecuteDelete,
                                                              async () => updatedRecords == 1);
        }
        else
        {
            // this will throw exception if no record or more than one record is affected
            // so don't need to rethrow here

            await UpdateAsync(conditionExpression,
                              new
                              {
                                  IsDeleted      = true,
                                  DeletedDate    = DateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow,
                                  DeletedBy      = CurrentLoggedInUserResolver?.CurrentUserName,
                                  DeletionReason = reason
                              },
                              expectedAffectedRows: 1);
        }

        // if no issue thus far, publish the event
        if (EventPublisher is not null)
        {
            _ = EventPublisher.Publish(new EntityDeletedByExpression<T>
            {
                DeletionReason   = reason,
                FilterExpression = conditionExpression,
                UserName         = CurrentLoggedInUserResolver?.CurrentUserName,
                UserId           = CurrentLoggedInUserResolver?.CurrentUserId,
                IsHardDeleted    = forceHardDelete
            });
        }
    }

    public virtual int DeleteMany<T>(Expression<Func<T, bool>>? conditionExpression,
                                     string?                    reason          = null,
                                     bool                       forceHardDelete = false)
        where T : class => DeleteManyAsync(conditionExpression, reason, forceHardDelete).Result;

    public virtual async Task<int> DeleteManyAsync<T>(Expression<Func<T, bool>>? conditionExpression,
                                                      string?                    reason          = null,
                                                      bool                       forceHardDelete = false)
        where T : class
    {
        const string isDeletedFieldName = nameof(IAuditableEntity.IsDeleted);

        forceHardDelete =
            forceHardDelete || !typeof(T).HasProperty<bool>(isDeletedFieldName);

        var updatedRecords = forceHardDelete
                                 ? await Fetch(conditionExpression).ExecuteDeleteAsync()
                                 : await UpdateAsync(conditionExpression,
                                                     new
                                                     {
                                                         IsDeleted      = true,
                                                         DeletedDate    = DateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow,
                                                         DeletedBy      = CurrentLoggedInUserResolver?.CurrentUserName,
                                                         DeletionReason = reason
                                                     },
                                                     expectedAffectedRows: null);

        if (EventPublisher is not null)
        {
            _ = EventPublisher.Publish(new EntityDeletedByExpression<T>
            {
                DeletionReason   = reason,
                FilterExpression = conditionExpression,
                AffectedRecords  = updatedRecords,
                UserName         = CurrentLoggedInUserResolver?.CurrentUserName,
                UserId           = CurrentLoggedInUserResolver?.CurrentUserId,
                IsHardDeleted    = forceHardDelete
            });
        }

        return updatedRecords;
    }

    public void RestoreOne<T>(Expression<Func<T, bool>>? conditionExpression) where T : class
        => RestoreOneAsync(conditionExpression).Wait();

    public async Task RestoreOneAsync<T>(Expression<Func<T, bool>>? conditionExpression) where T : class
    {
        Guards.AssertEntityRecoverable<T>();

        await UpdateAsync(conditionExpression,
                          new
                          {
                              IsDeleted      = false,
                              DeletedDate    = default(DateTimeOffset?),
                              DeletedBy      = default(string),
                              DeletionReason = default(string)
                          },
                          expectedAffectedRows: 1);
    }

    public virtual int RestoreMany<T>(Expression<Func<T, bool>>? conditionExpression)
        where T : class => RestoreManyAsync(conditionExpression).Result;

    public virtual async Task<int> RestoreManyAsync<T>(Expression<Func<T, bool>>? conditionExpression)
        where T : class
    {
        Guards.AssertEntityRecoverable<T>();

        var updatedRecords = await UpdateAsync(conditionExpression,
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

    public int CommitChanges() => CommitChangesAsync().Result;

    public async Task<int> CommitChangesAsync()
    {
        List<EntityEntry> insertedEntities = [];
        List<EntityEntry> updatedEntities  = [];

        try
        {
            if (!DbContext.ChangeTracker.HasChanges())
            {
                return 0;
            }

            await OnBeforeSaveChanges(insertedEntities, updatedEntities);

            return await DbContext.SaveChangesAsync();
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
            if (insertedEntities.Count != 0 ||
                updatedEntities.Count != 0)
            {
                await OnAfterSaveChanges(insertedEntities, updatedEntities);
            }
        }
    }


    protected virtual async Task OnBeforeSaveChanges(List<EntityEntry> insertedEntities,
                                                     List<EntityEntry> updatedEntities)
    {
        if (!DbContext.ChangeTracker.HasChanges())
        {
            return;
        }

        var entityEntries = DbContext.ChangeTracker
                                     .Entries()
                                     .ToArray();

        insertedEntities.AddRange(entityEntries.Where(e => e.State == EntityState.Added));

        updatedEntities.AddRange(entityEntries.Where(e => e.State == EntityState.Modified ||
                                                          e.State == EntityState.Deleted));


        var eventMessage = new DbContextBeforeSaveChanges
        {
            InsertedEntityEntries = insertedEntities,
            UpdatedEntityEntries  = updatedEntities,
            CurrentUserId         = CurrentLoggedInUserResolver?.CurrentUserId,
            CurrentUserName       = CurrentLoggedInUserResolver?.CurrentUserName,
        };

        DbOnBeforeSaveChanges_SetAuditInformation.HandleEvent(eventMessage, DateTimeProvider, Logger);

        if (EventPublisher is not null)
            await EventPublisher.Publish(eventMessage);
    }

    protected virtual async Task OnAfterSaveChanges(List<EntityEntry> insertedEntities, List<EntityEntry> updatedEntities)
    {
        if (insertedEntities.Count == 0 &&
            updatedEntities.Count == 0)
        {
            return;
        }

        if (EventPublisher is not null)
        {
            await EventPublisher.Publish(new DbContextAfterSaveChanges
            {
                InsertedEntityEntries = insertedEntities,
                UpdatedEntityEntries  = updatedEntities,
                CurrentUserId         = CurrentLoggedInUserResolver?.CurrentUserId,
                CurrentUserName       = CurrentLoggedInUserResolver?.CurrentUserName,
            });
        }
    }

    private SetPropertyBuilder<T> PrepareUpdatePropertiesBuilder<T>(Expression<Func<T, T>> updateExpression)
    {
        if (updateExpression.Body is not MemberInitExpression memberInitExpression)
            throw new InvalidOperationException("Invalid expression type for updating entity");

        var memberAssignmentList = new List<MemberBinding>(memberInitExpression.Bindings);

        var setPropBuilder = new SetPropertyBuilder<T>();

        bool hasModifiedDate = false, hasModifiedBy = false;

        foreach (var binding in memberAssignmentList.OfType<MemberAssignment>())
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
                                                              DateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow);
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
        DbContext.Dispose();
    }
}
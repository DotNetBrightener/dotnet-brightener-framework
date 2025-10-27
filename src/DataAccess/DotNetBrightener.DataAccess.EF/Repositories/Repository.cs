#nullable enable

using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.DataAccess.EF.Extensions;
using DotNetBrightener.DataAccess.EF.Internal;
using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.DataAccess.Exceptions;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Models.Auditing;
using DotNetBrightener.DataAccess.Models.Guards;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.DataAccess.Utils;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetBrightener.DataAccess.EF.Repositories;

public class Repository(
    DbContext        dbContext,
    IServiceProvider serviceProvider,
    ILoggerFactory   loggerFactory)
    : ReadOnlyRepository(dbContext, serviceProvider, loggerFactory), IRepository
{
    private readonly IgnoreAuditingEntitiesContainer? _ignoreAuditingEntitiesContainer =
        serviceProvider.TryGet<IgnoreAuditingEntitiesContainer>();

    private readonly IAuditEntriesProcessor? _auditEntriesProcessor = serviceProvider.TryGet<IAuditEntriesProcessor>();

    public virtual void Insert<T>(T entity)
        where T : class => InsertAsync(entity).Wait();

    public virtual async Task InsertAsync<T>(T entity)
        where T : class
    {
        if (EventPublisher is not null)
        {
            await EventPublisher.Publish(new EntityCreating<T>
            {
                UserName = CurrentLoggedInUserResolver?.CurrentUserName,
                UserId   = CurrentLoggedInUserResolver?.CurrentUserId,
                Entity   = entity
            });
        }

        await DbContext.AddAsync(entity);
    }

    public virtual void InsertMany<T>(params IEnumerable<T> entities)
        where T : class => InsertManyAsync(entities).Wait();

    public virtual async Task InsertManyAsync<T>(params IEnumerable<T> entities)
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

        await DbContext.AddRangeAsync(entitiesToInserts);
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

        return await LinqToDB.LinqExtensions.InsertAsync(query,
                                                         DbContext.Set<TTarget>().ToLinqToDBTable(),
                                                         copyExpression);
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

        DbContext.Update(entity);
    }

    public virtual void UpdateMany<T>(IEnumerable<T> entities) where T : class =>
        UpdateMany(entities.ToArray());

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

    public Task<int> UpdateOneAsync<T>(Expression<Func<T, bool>>? conditionExpression,
                                       Expression<Func<T, T>>     updateExpression)
        where T : class => UpdateAsync(conditionExpression, updateExpression, expectedAffectedRows: 1);

    public virtual async Task<int> UpdateAsync<T>(Expression<Func<T, bool>>? conditionExpression,
                                                  Expression<Func<T, T>>     updateExpression,
                                                  int?                       expectedAffectedRows = null)
        where T : class
    {
        int updatedRecords = 0;


        var executionStrategy = DbContext.Database.CreateExecutionStrategy();

        async Task ExecuteUpdate()
        {
            updatedRecords = await PerformUpdate(conditionExpression,
                                                 updateExpression,
                                                 expectedAffectedRows);

            if (expectedAffectedRows is not null &&
                updatedRecords != expectedAffectedRows.Value)
            {
                throw new ExpectedAffectedRecordMismatchException(expectedAffectedRows.Value,
                                                                  updatedRecords);
            }
        }

        await executionStrategy.ExecuteInTransactionAsync(ExecuteUpdate,
                                                          async () => expectedAffectedRows is null ||
                                                                      updatedRecords == expectedAffectedRows.Value);


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

            DbContext.Update(entity);
        }
        else
        {
            DbContext.Remove(entity);
        }
    }

    private async Task<int> PerformUpdate<T>(Expression<Func<T, bool>>? conditionExpression,
                                             Expression<Func<T, T>>     updateExpression,
                                             int?                       expectedAffectedRows = null)
        where T : class
    {
        SetPropertyBuilder<T> updateQueryBuilder = PrepareUpdatePropertiesBuilder(updateExpression);

        var url           = HttpContextAccessor?.HttpContext?.Request.GetDisplayUrl();
        var requestMethod = HttpContextAccessor?.HttpContext?.Request.Method;

        var start = DateTimeOffset.UtcNow;

        var query = conditionExpression is not null
                        ? DbContext.Set<T>().Where(conditionExpression)
                        : DbContext.Set<T>();

        int result = await query.ExecutePatchUpdateAsync(updateQueryBuilder);

        if (_ignoreAuditingEntitiesContainer?.Contains(typeof(T)) != true &&
            result > 0 &&
            _auditEntriesProcessor is not null &&
            (expectedAffectedRows is null ||
             expectedAffectedRows == result))
        {
            var now                    = DateTimeOffset.Now;
            var extractAuditProperties = updateQueryBuilder.ExtractAuditProperties();
            var auditEntity = new AuditEntity
            {
                ScopeId         = ScopeId,
                StartTime       = start,
                EndTime         = now,
                Duration        = now.Subtract(start),
                Action          = updateQueryBuilder.ActionName,
                EntityType      = typeof(T).Name,
                Url             = $"{requestMethod} {url}".Trim(),
                UserName        = CurrentLoggedInUserResolver?.CurrentUserName ?? "Not Detected",
                AuditProperties = extractAuditProperties,
                IsSuccess       = true,
                AffectedRows    = result
            };

            auditEntity.PrepareDebugView();


            try
            {
                var entityIdentifier = conditionExpression.ExtractFilters();

                auditEntity.EntityIdentifier = entityIdentifier.Serialize();
            }
            catch (NotSupportedException)
            {

            }

            await _auditEntriesProcessor.QueueAuditEntries(auditEntity);
        }

        return result;
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

            DateTimeOffset start = DateTimeOffset.UtcNow;

            await executionStrategy.ExecuteInTransactionAsync(ExecuteDelete,
                                                              async () => updatedRecords == 1);

            if (_ignoreAuditingEntitiesContainer?.Contains(typeof(T)) != true &&
                _auditEntriesProcessor is not null &&
                updatedRecords == 1)
            {
                var url           = HttpContextAccessor?.HttpContext?.Request.GetDisplayUrl();
                var requestMethod = HttpContextAccessor?.HttpContext?.Request.Method;

                Logger.LogDebug("Initializing Audit Entity");

                var entityIdentifier = conditionExpression.ExtractFilters();

                var now = DateTimeOffset.UtcNow;
                var auditEntity = new AuditEntity
                {
                    ScopeId          = ScopeId,
                    StartTime        = start,
                    EndTime          = now,
                    Duration         = now.Subtract(start),
                    Action           = "Hard-Deleted using Expression",
                    EntityIdentifier = entityIdentifier.Serialize(),
                    EntityType       = typeof(T).Name,
                    Url              = $"{requestMethod} {url}".Trim(),
                    UserName         = CurrentLoggedInUserResolver?.CurrentUserName ?? "Not Detected",
                    IsSuccess        = true,
                    AffectedRows     = updatedRecords
                };
                await _auditEntriesProcessor.QueueAuditEntries(auditEntity);
            }
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
                                  UserName       = CurrentLoggedInUserResolver?.CurrentUserName ?? "Not Detected",
                                  DeletionReason = reason
                              },
                              expectedAffectedRows: 1);
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

        int updatedRecords;

        if (forceHardDelete)
        {
            DateTimeOffset start = DateTimeOffset.UtcNow;
            updatedRecords = await Fetch(conditionExpression).ExecuteDeleteAsync();

            if (updatedRecords != 0 &&
                _ignoreAuditingEntitiesContainer?.Contains(typeof(T)) != true &&
                _auditEntriesProcessor is not null)
            {
                var url           = HttpContextAccessor?.HttpContext?.Request.GetDisplayUrl();
                var requestMethod = HttpContextAccessor?.HttpContext?.Request.Method;

                Logger.LogDebug("Initializing Audit Entity");

                var entityIdentifier = conditionExpression.ExtractFilters();

                var now = DateTimeOffset.UtcNow;
                var auditEntity = new AuditEntity
                {
                    ScopeId          = ScopeId,
                    StartTime        = start,
                    EndTime          = now,
                    Duration         = now.Subtract(start),
                    Action           = "Hard-Deleted using Expression",
                    EntityIdentifier = entityIdentifier.Serialize(),
                    EntityType       = typeof(T).Name,
                    Url              = $"{requestMethod} {url}".Trim(),
                    UserName         = CurrentLoggedInUserResolver?.CurrentUserName ?? "Not Detected",
                    IsSuccess        = true,
                    AffectedRows     = updatedRecords
                };

                await _auditEntriesProcessor.QueueAuditEntries(auditEntity);
            }

            return updatedRecords;
        }

        updatedRecords = await UpdateAsync(conditionExpression,
                                           new
                                           {
                                               IsDeleted = true,
                                               DeletedDate =
                                                   DateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow,
                                               DeletedBy      = CurrentLoggedInUserResolver?.CurrentUserName,
                                               DeletionReason = reason
                                           },
                                           expectedAffectedRows: null);

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
        try
        {
            if (!DbContext.ChangeTracker.HasChanges())
            {
                return 0;
            }

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
    }

    public IAsyncDisposable BeginUnitOfWork()
    {
        return new RepositoryUnitOfWork(DbContext, Logger);
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
            typeof(T).HasProperty<DateTimeOffset?>(nameof(IAuditableEntity.ModifiedDate)))
        {
            setPropBuilder.SetPropertyByName<DateTimeOffset?>(nameof(IAuditableEntity.ModifiedDate),
                                                              DateTimeProvider?.UtcNowWithOffset ??
                                                              DateTimeOffset.UtcNow);
        }

        if (!hasModifiedBy &&
            typeof(T).HasProperty<string>(nameof(IAuditableEntity.ModifiedBy)))
        {
            setPropBuilder.SetPropertyByName(nameof(IAuditableEntity.ModifiedBy),
                                             CurrentLoggedInUserResolver?.CurrentUserName ?? "[Not Detected]");
        }

        return setPropBuilder;
    }

    private class RepositoryUnitOfWork(
        DbContext dbContext,
        ILogger   logger) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error while saving changes to database");

                throw;
            }
        }
    }
}
using DotNetBrightener.Core.DataAccess.Abstractions;
using DotNetBrightener.Core.DataAccess.Abstractions.Exceptions;
using DotNetBrightener.Core.DataAccess.Abstractions.Repositories;
using DotNetBrightener.Core.DataAccess.Abstractions.Transaction;
using DotNetBrightener.Core.DataAccess.Extensions;
using DotNetBrightener.Core.DataAccess.Providers;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetBrightener.Core.DataAccess.Repositories
{
    public class Repository : IRepository
    {
        protected readonly IDataWorkContext    DataWorkContext;
        protected readonly ITransactionManager TransactionManager;
        protected readonly ILogger             Logger;

        /// <summary>
        ///     Retrieves the current logged in user's name for audit purpose
        /// </summary>
        protected string CurrentLoggedInUser => DataWorkContext.GetContextData<string>(CommonUserConstants.CurrentLoggedInUserName);

        /// <summary>
        ///     Retrieves the current logged in user's id for audit purpose
        /// </summary>
        protected long? CurrentLoggedInUserId => DataWorkContext.GetContextData<long?>(CommonUserConstants.CurrentLoggedInUserId);
        
        protected readonly IDotNetBrightenerDataProvider DataProvider;
        protected readonly DataConnection DataContext;

        public Repository(DatabaseConfiguration databaseConfiguration,
                          IServiceProvider      serviceProvider,
                          IDataWorkContext      dataWorkContext,
                          ITransactionManager   transactionManager,
                          IDataProviderFactory  dataProviderFactory,
                          ILogger<Repository>   logger)
        {
            TransactionManager = transactionManager;
            DataWorkContext    = dataWorkContext;
            Logger             = logger;
            DataProvider       = dataProviderFactory.GetDataProvider();

            if (DataProvider == null)
                throw new
                    NotSupportedException($"The specified database provider {databaseConfiguration.DatabaseProvider} is not supported by the system");

            DataContext = DataProvider.CreateDataConnection(databaseConfiguration.ConnectionString);

            DataContext.AddMappingSchema(new MappingSchema(DataContext.DataProvider.Name)
            {
                MetadataReader = MetadataReader.RetrieveInstance(serviceProvider)
            });
        }

        public virtual Task<T> Get<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return Fetch(expression).SingleOrDefaultAsync();
        }

        public virtual Task<TResult> Get<T, TResult>(Expression<Func<T, bool>>    expression,
                                                     Expression<Func<T, TResult>> propertiesPickupExpression) where T : class
        {
            return Fetch(expression, propertiesPickupExpression).SingleOrDefaultAsync();
        }

        public virtual IQueryable<T> Fetch<T>(Expression<Func<T, bool>> expression = null) where T : class
        {
            if (expression == null)
                return DataContext.GetTable<T>();

            return DataContext.GetTable<T>()
                              .Where(expression);
        }

        public virtual IQueryable<TResult> Fetch<T, TResult>(Expression<Func<T, bool>> expression,
                                                             Expression<Func<T, TResult>> propertiesPickupExpression) where T : class
        {
            return Fetch(expression).Select(propertiesPickupExpression);
        }

        public virtual IQueryable<T> OrderedFetch<T>(Expression<Func<T, bool>> expression = null,
                                                     Expression<Func<T, object>> orderExpression = null,
                                                     int? pageSize = null,
                                                     int? pageIndex = null) where T : class
        {
            var query = Fetch(expression);

            if (orderExpression != null)
            {
                var orderedQuery = query.OrderBy(orderExpression);

                if (pageSize != null)
                {
                    if (pageIndex != null)
                    {
                        return orderedQuery.Skip(pageSize.Value * pageIndex.Value)
                                           .Take(pageSize.Value);
                    }

                    return orderedQuery.Take(pageSize.Value);
                }

                return orderedQuery;
            }

            return query;
        }

        public IQueryable<TResult> OrderedFetch<T, TResult>(Expression<Func<T, bool>> expression = null,
                                                            Expression<Func<T, TResult>> propertiesPickupExpression = null,
                                                            Expression<Func<T, object>> orderExpression = null,
                                                            int? pageSize = null,
                                                            int? pageIndex = null)
            where T : class
        {

            return OrderedFetch(expression, orderExpression, pageSize, pageIndex).Select(propertiesPickupExpression);
        }

        public virtual IQueryable<T> OrderedDescendingFetch<T>(Expression<Func<T, bool>> expression = null,
                                                               Expression<Func<T, object>> orderExpression = null,
                                                               int? pageSize = null,
                                                               int? pageIndex = null)
            where T : class
        {
            var query = Fetch(expression);

            if (orderExpression != null)
            {
                var orderedQuery = query.OrderByDescending(orderExpression);

                if (pageSize != null)
                {
                    if (pageIndex != null)
                    {
                        return orderedQuery.Skip(pageSize.Value * pageIndex.Value)
                                           .Take(pageSize.Value);
                    }

                    return orderedQuery.Take(pageSize.Value);
                }

                return orderedQuery;
            }

            return query;
        }

        public virtual Task<int> Count<T>(Expression<Func<T, bool>> expression = null) where T : class
        {
            return Fetch(expression).CountAsync();
        }

        public virtual Task Insert<T>(T entity) where T : class
        {
            var identityFields = typeof(T).GetProperties()
                                         .Where(_ => _.GetCustomAttribute<KeyAttribute>() != null)
                                         .ToArray();

            object InternalInsert()
            {
                return DataContext.InsertWithIdentity(entity);
            }

            void InternalInsertWithoutIdentity()
            {
                DataContext.Insert(entity);
            }

            if (!string.IsNullOrEmpty(CurrentLoggedInUser))
                entity.TryAssignValue(nameof(BaseEntityWithAuditInfo.CreatedBy), CurrentLoggedInUser);

            entity.TryAssignValue(nameof(BaseEntityWithAuditInfo.Created), DateTimeOffset.Now);

            if (identityFields.Length == 1)
            {
                var identityField = identityFields.First();
                var databaseGeneratedAttr = identityField.GetCustomAttribute<DatabaseGeneratedAttribute>();
                if (databaseGeneratedAttr?.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                {
                    var outputIdentity = InternalInsert();
                    try
                    {
                        identityField.SetValue(entity, outputIdentity);
                    }
                    catch
                    {
                        // cannot set the value
                    }

                    return Task.CompletedTask;
                }
            }

            InternalInsertWithoutIdentity();


            return Task.CompletedTask;
        }

        public virtual Task Insert<T>(IEnumerable<T> entities) where T : class
        {
            var entitiesToInsert = entities.ToArray();

            foreach (var entity in entitiesToInsert)
            {
                if (!string.IsNullOrEmpty(CurrentLoggedInUser))
                    entity.TryAssignValue(nameof(BaseEntityWithAuditInfo.CreatedBy), CurrentLoggedInUser);

                entity.TryAssignValue(nameof(BaseEntityWithAuditInfo.Created), DateTimeOffset.Now);
            }

            var result = DataContext.BulkCopy(entitiesToInsert);

            var affectedRecords = result.RowsCopied;

            var expectedAffectedRows = entitiesToInsert.Length;

            if (affectedRecords != expectedAffectedRows)
            {
                throw new
                    ExpectedAffectedRecordMismatchException($"Expected {expectedAffectedRows} rows to be inserted but was {affectedRecords}.", expectedAffectedRows, (int)affectedRecords);
            }

            return Task.CompletedTask;
        }

        public virtual int CopyRecords<TSource, TTarget>(Expression<Func<TSource, bool>>    conditionExpression,
                                                         Expression<Func<TSource, TTarget>> copyExpression)
            where TSource : class
            where TTarget : class
        {
            var sources = DataContext.GetTable<TSource>().Where(conditionExpression);
            var target  = DataContext.GetTable<TTarget>();

            return sources.Insert(target, copyExpression);
        }

        public virtual int Update<T>(Expression<Func<T, bool>> conditionExpression,
                                     object updateExpression,
                                     int? expectedAffectedRows = null)
            where T : class
        {
            var updateRecordExpression = BuildMemberUpdateExpressionFromObject<T, T>(updateExpression);

            return Update(conditionExpression, updateRecordExpression, expectedAffectedRows);
        }

        public virtual int Update<T>(Expression<Func<T, bool>> conditionExpression,
                                     Expression<Func<T, T>>    updateExpression,
                                     int?                      expectedAffectedRows = null) where T : class

        {
            updateExpression = AppendAuditInfoToExpression(updateExpression);

            var affectedRecords = DataContext.GetTable<T>()
                                             .Update(conditionExpression, updateExpression);

            if (expectedAffectedRows.HasValue && affectedRecords != expectedAffectedRows)
            {
                throw new
                    ExpectedAffectedRecordMismatchException($"Expected {expectedAffectedRows} rows to be updated but {affectedRecords} affected. ",  expectedAffectedRows, affectedRecords);
            }

            return affectedRecords;
        }

        public virtual Task DeleteOne<T>(Expression<Func<T, bool>> conditionExpression, bool forceHardDelete = false)
            where T : class
        {
            if (!forceHardDelete)
            {
                forceHardDelete = !typeof(T).HasProperty<bool>(nameof(BaseEntityWithAuditInfo.IsDeleted));
            }


            int recordsAffected;

            if (forceHardDelete)
            {
                recordsAffected = DataContext.GetTable<T>()
                                             .Delete(conditionExpression);
            }
            else
            {
                var updateRecordExpression = BuildMemberUpdateExpressionFromObject<T, T>(new
                {
                    IsDeleted = true,
                    Deleted   = DateTimeOffset.Now
                });
                recordsAffected = DataContext.GetTable<T>()
                                             .Update(conditionExpression, updateRecordExpression);
            }

            if (recordsAffected != 1)
            {
                throw new
                    ExpectedAffectedRecordMismatchException($"Expecting one record to delete but {recordsAffected} were found. The operation has been rolled back.", 1, recordsAffected);
            }

            return Task.FromResult(recordsAffected);
        }

        public virtual Task<int> DeleteMany<T>(Expression<Func<T, bool>> conditionExpression,
                                               bool forceHardDelete = false) where T : class
        {

            if (!forceHardDelete)
            {
                forceHardDelete = !typeof(T).HasProperty<bool>(nameof(BaseEntityWithAuditInfo.IsDeleted));
            }

            int recordsAffected;

            if (forceHardDelete)
            {
                recordsAffected = DataContext.GetTable<T>()
                                             .Delete(conditionExpression);
            }
            else
            {
                var updateRecordExpression = BuildMemberUpdateExpressionFromObject<T, T>(new
                {
                    IsDeleted = true,
                    Deleted   = DateTimeOffset.Now
                });
                recordsAffected = DataContext.GetTable<T>()
                                             .Update(conditionExpression, updateRecordExpression);
            }

            return Task.FromResult(recordsAffected);
        }

        public virtual Task<IEnumerable<T>> ExecuteQuery<T>(string sql, params SqlParameter[] parameters)
            where T : class
        {
            var command = DataContext.CreateCommand();
            command.CommandText = sql;
            foreach (var parameter in parameters)
            {
                var commandParam = command.CreateParameter();
                commandParam.Value = parameter.Value;
                commandParam.ParameterName = parameter.ParameterName;
                command.Parameters.Add(command);
            }

            return Task.FromResult(command.ExecuteReader(CommandBehavior.CloseConnection)
                                          .MapToList<T>()
                                          .AsEnumerable());
        }

        public virtual TResult ExecuteScala<TResult>(string sql, params SqlParameter[] parameters)
        {
            var command = DataContext.CreateCommand();
            command.CommandText = sql;
            foreach (var parameter in parameters)
            {
                var commandParam = command.CreateParameter();
                commandParam.Value         = parameter.Value;
                commandParam.ParameterName = parameter.ParameterName;
                command.Parameters.Add(command);
            }

            return command.ExecuteScalar() is TResult tResult ? tResult : default;
        }

        public virtual int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            var command = DataContext.CreateCommand();
            command.CommandText = sql;
            foreach (var parameter in parameters)
            {
                var commandParam = command.CreateParameter();
                commandParam.Value         = parameter.Value;
                commandParam.ParameterName = parameter.ParameterName;
                command.Parameters.Add(command);
            }

            return command.ExecuteNonQuery();
        }

        public virtual Task<T> RunInTransaction<T>(Func<T> action)
        {
            using var transaction = TransactionManager.BeginTransaction();
            try
            {
                var result = action();

                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }

        public virtual async Task<T> RunInTransaction<T>(Func<Task<T>> action) where T : struct
        {
            using var transaction = TransactionManager.BeginTransaction();
            try
            {
                var result = await action();

                return result;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }

        public virtual Task RunInTransaction(Action action)
        {
            using var transaction = TransactionManager.BeginTransaction();
            try
            {
                return Task.Run(action);
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }

        public virtual void Dispose()
        {
            DataContext.Dispose();
        }

        /// <summary>
        ///     Generates the expression to describe how to update an existing record from the given object
        /// </summary>
        /// <remarks>
        ///     This method also retrieves the current logged in user information and save as audit information
        /// </remarks>
        /// <param name="dataTransferObject">
        ///     The object contains the information to build the expression
        /// </param>
        /// <returns>
        ///     The expression describes how to update the object
        /// </returns>
        public Expression<Func<TIn, TOut>> BuildMemberUpdateExpressionFromObject<TIn, TOut>(object dataTransferObject)
        {
            return BuildMemberInitExpressionFromObject<TIn, TOut>(dataTransferObject, false);
        }

        /// <summary>
        ///     Generates the expression to describe how to initiate or update a record from the given object
        /// </summary>
        /// <remarks>
        ///     This method also retrieves the current logged in user information and save as audit information
        /// </remarks>
        /// <param name="dataTransferObject">
        ///     The object contains the information to build the expression
        /// </param>
        /// <param name="creationMode">
        ///     Indicates the expression is used for creation of record.
        ///     Specify <c>true</c> to indicate that the expression is used for creating an new record.
        ///     Specify <c>false</c> to indicate that the expression is used for updating an existing record.
        /// </param>
        /// <returns>
        ///     The expression describes how to create the object
        /// </returns>
        public Expression<Func<TIn, TOut>> BuildMemberInitExpressionFromObject<TIn, TOut>(object dataTransferObject, bool creationMode)
        {
            var destinationType = typeof(TIn);
            var constructorInfo = destinationType.GetConstructor(new Type[0]);

            if (constructorInfo == null)
            {
                throw new InvalidOperationException($"Cannot find the constructor of the target type");
            }

            var newExpression        = Expression.New(constructorInfo);
            var memberAssignmentList = new List<MemberAssignment>();

            var dtoType = dataTransferObject.GetType();

            // we are not converting some properties that should not be put back to the entity
            var sourceProps = dtoType.GetProperties()
                                     .Where(_ => !_.HasAttribute<NotMappedAttribute>())
                                     .ToArray();

            foreach (var propertyInfo in sourceProps)
            {
                var destinationProp = destinationType.GetProperty(propertyInfo.Name);

                // if the property also should not be mapped into the database or it is a key
                if (destinationProp == null ||
                    destinationProp.HasAttribute<NotMappedAttribute>())
                    continue;

                if (destinationProp.HasAttribute<KeyAttribute>())
                {
                    var keyPropAttr = destinationProp.GetCustomAttribute<DatabaseGeneratedAttribute>();
                    if (keyPropAttr == null ||
                        keyPropAttr.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                        continue;
                }

                var value = propertyInfo.GetValue(dataTransferObject);

                if (value != null)
                {
                    if (destinationProp.PropertyType == typeof(DateTime) &&
                        value is DateTime dateTimeValue &&
                        dateTimeValue == DateTime.MinValue)
                    {
                        value = new DateTime(1970, 1, 1);
                    }

                    memberAssignmentList.Add(Expression.Bind(
                                                             destinationProp,
                                                             Expression.Constant(value, destinationProp.PropertyType)
                                                            )
                                            );
                }
            }

            // assign value to field CreatedBy / LastUpdateBy
            var updatedByUserFieldName = creationMode
                                             ? nameof(BaseEntityWithAuditInfo.CreatedBy)
                                             : nameof(BaseEntityWithAuditInfo.LastUpdatedBy);
            var updatedByUserField = destinationType.GetProperty(updatedByUserFieldName);
            if (updatedByUserField != null &&
                !string.IsNullOrEmpty(CurrentLoggedInUser))
            {
                memberAssignmentList.Add(Expression.Bind(
                                                         updatedByUserField,
                                                         Expression.Constant(CurrentLoggedInUser,
                                                                             updatedByUserField.PropertyType)
                                                        )
                                        );
            }

            // assign value to field Created/LastUpdate
            var lastUpdateFieldName = creationMode
                                          ? nameof(BaseEntityWithAuditInfo.Created)
                                          : nameof(BaseEntityWithAuditInfo.LastUpdated);

            var updatedDateField = destinationType.GetProperty(lastUpdateFieldName);
            if (updatedDateField != null)
            {
                memberAssignmentList.Add(Expression.Bind(
                                                         updatedDateField,
                                                         Expression.Constant(DateTimeOffset.Now,
                                                                             updatedDateField.PropertyType)
                                                        )
                                        );
            }

            Expression<Func<TIn, TOut>> memberInitFactory =
                Expression.Lambda<Func<TIn, TOut>>(Expression.MemberInit(newExpression, memberAssignmentList),
                                                   Expression.Parameter(destinationType));

            return memberInitFactory;
        }

        private Expression<Func<T, T>> AppendAuditInfoToExpression<T>(Expression<Func<T, T>> updateExpression) where T : class
        {
            if (!(updateExpression.Body is MemberInitExpression memberInitExpression))
            {
                return updateExpression;
            }

            var memberAssignments = memberInitExpression.Bindings.OfType<MemberAssignment>();

            if (!typeof(T).HasProperty<DateTimeOffset?>(nameof(BaseEntityWithAuditInfo.LastUpdated)) && 
                !typeof(T).HasProperty<string>(nameof(BaseEntityWithAuditInfo.LastUpdatedBy)))
            {
                return updateExpression;
            }

            var destinationType = typeof(T);
            var constructorInfo = destinationType.GetConstructor(new Type[0]);

            if (constructorInfo == null)
            {
                throw new InvalidOperationException($"Cannot find the constructor of the target type");
            }

            var newExpression = Expression.New(constructorInfo);
            var memberAssignmentList = new List<MemberAssignment>();

            foreach (var memberBinding in memberAssignments)
            {
                if (memberBinding.Member.Name == nameof(BaseEntityWithAuditInfo.LastUpdated) ||
                    memberBinding.Member.Name == nameof(BaseEntityWithAuditInfo.LastUpdatedBy))
                    continue;

                memberAssignmentList.Add(memberBinding);
            }

            // assign value to field LastUpdateBy
            var updatedByUserFieldName = nameof(BaseEntityWithAuditInfo.LastUpdatedBy);
            var updatedByUserField = destinationType.GetProperty(updatedByUserFieldName);
            if (updatedByUserField != null &&
                !string.IsNullOrEmpty(CurrentLoggedInUser))
            {
                memberAssignmentList.Add(Expression.Bind(
                                                         updatedByUserField,
                                                         Expression.Constant(CurrentLoggedInUser, updatedByUserField.PropertyType)
                                                        )
                                        );
            }

            // assign value to field LastUpdate
            var lastUpdateFieldName = nameof(BaseEntityWithAuditInfo.LastUpdated);

            var updatedDateField = destinationType.GetProperty(lastUpdateFieldName);
            if (updatedDateField != null)
            {
                memberAssignmentList.Add(Expression.Bind(
                                                         updatedDateField,
                                                         Expression.Constant(DateTimeOffset.Now, updatedDateField.PropertyType)
                                                        )
                                        );
            }

            Expression<Func<T, T>> memberInitFactory =
                Expression.Lambda<Func<T, T>>(Expression.MemberInit(newExpression, memberAssignmentList),
                                                   Expression.Parameter(destinationType));

            return memberInitFactory;
        }
    }
}
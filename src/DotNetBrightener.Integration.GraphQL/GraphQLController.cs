using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DotNetBrightener.Integration.GraphQL.Attributes;
using DotNetBrightener.Integration.GraphQL.Transactions;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Integration.GraphQL
{
    public abstract class GraphQLController : ObjectGraphType
    {
        protected GraphQLController()
        {
            var graphQLPublicMethods = GetType()
                                      .GetMethods()
                                      .Where(_ => _.HasAttribute<GraphQLMethodAttribute>());

            foreach (var method in graphQLPublicMethods)
            {
                var methodAttribute    = method.GetCustomAttribute<GraphQLMethodAttribute>();
                var argumentAttributes = method.GetCustomAttributes<GraphQLArgumentAttribute>();

                var argumentBuilder = new ArgumentBuilder();
                foreach (var argument in argumentAttributes)
                {
                    argumentBuilder.AddArgument(argument.ArgumentName, argument.ArgumentType);
                }

                var isAwaitable = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

                if (isAwaitable)
                {
                    FieldAsync(methodAttribute.ReturnType,
                               methodAttribute.Name,
                               method.Description(),
                               argumentBuilder.Finalize(),
                               context => ResolveMethod(method, context),
                               method.ObsoleteMessage());
                }
                else
                {
                    Field(methodAttribute.ReturnType,
                          methodAttribute.Name,
                          method.Description(),
                          argumentBuilder.Finalize(),
                          context => ResolveMethod(method, context).Result,
                          method.ObsoleteMessage());
                }
            }
        }

        protected PaginationViewModel<TEntity, TEntityGraphType> PaginateQuery<TEntity, TEntityGraphType>(
            IQueryable<TEntity> query,
            int pageSize = 0,
            int pageIndex = 0,
            string orderBy = "",
            string orderDir = "asc")
            where TEntity : class
        {
            var orderByExpression = ExpressionExtensions.BuildMemberAccessExpression<TEntity>("Id");

            if (!string.IsNullOrEmpty(orderBy))
            {
                orderByExpression = ExpressionExtensions.BuildMemberAccessExpression<TEntity>(orderBy);
                query = orderDir == "asc"
                            ? query.OrderBy(orderByExpression)
                            : query.OrderByDescending(orderByExpression);
            }
            else
            {
                query = query.OrderBy(orderByExpression);
            }

            var totalRecords = query.Count();

            if (pageSize > 0)
            {
                query = query.Skip(pageSize * pageIndex)
                             .Take(pageSize);
            }

            return new PaginationViewModel<TEntity, TEntityGraphType>
            {
                TotalRecords = totalRecords,
                PageSize = pageSize,
                PageIndex = pageIndex,
                Data = query
            };
        }

        private async Task<object> ResolveMethod(MethodInfo method,
                                                 IResolveFieldContext<object> context)
        {
            var parameters = new List<object> { };

            var methodParams = method.GetParameters();

            if (methodParams.Length == 1 && methodParams.FirstOrDefault().ParameterType == context.GetType())
            {
                parameters.Add(context);
            }
            else
            {
                foreach (var parameterInfo in methodParams)
                {
                    var parameter = context.GetArgument(parameterInfo.ParameterType, parameterInfo.Name);
                    parameters.Add(parameter);
                }
            }

            var isAwaitable = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

            async Task<object> ExecuteMethod()
            {
                if (isAwaitable)
                {
                    if (method.ReturnType.IsGenericType)
                    {
                        return await (dynamic) method.Invoke(this, parameters.ToArray());
                    }

                    throw new InvalidOperationException("The asynchronous operation does not return the correct data");
                }


                return method.Invoke(this, parameters.ToArray());
            }

            var logger = context.RequestServices.GetService<ILogger<GraphQLController>>();

            if (method.HasAttribute<GraphQLTransactionalOperationAttribute>())
            {
                var transactionManager = context.RequestServices.GetService<ITransactionManager>();

                object methodResult = null;
                using (var transaction = transactionManager.BeginTransaction())
                {
                    try
                    {
                        methodResult = await ExecuteMethod();
                    }
                    catch (Exception exception)
                    {
                        transaction.Rollback();
                        logger.LogError(new EventId(), exception, $"Error while executing GraphQL Method {this.GetType().FullName}.{method.Name}.");
                        context.Errors.Add(new ExecutionErrorWithStatusCode(exception.GetFullExceptionMessage()));
                        return null;
                    }
                }

                return methodResult;
            }

            try
            {
                return await ExecuteMethod();
            }
            catch (Exception exception)
            {
                logger.LogError(new EventId(), exception, $"Error while executing GraphQL Method {this.GetType().FullName}.{method.Name}.");
                context.Errors.Add(new ExecutionErrorWithStatusCode(exception.GetFullExceptionMessage()));
                return null;
            }
        }
    }

    internal class ExecutionErrorWithStatusCode : ExecutionError
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;

        public ExecutionErrorWithStatusCode(string message) : base(message)
        {
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DotNetBrightener.Integration.GraphQL.Attributes;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;

namespace DotNetBrightener.Integration.GraphQL
{
    public abstract class BaseGraphQLViewModel<TEntity> : ObjectGraphType<TEntity> where TEntity : class
    {
        private const string ORIGINAL_EXPRESSION_PROPERTY_NAME = nameof(ORIGINAL_EXPRESSION_PROPERTY_NAME);

        protected virtual string[] IgnoreFieldNames { get; } = { };

        protected BaseGraphQLViewModel()
        {
            Description       = GetType().Description();
            DeprecationReason = GetType().ObsoleteMessage();

            var properties = typeof(TEntity).GetProperties()
                                            .Where(_ => !_.GetSetMethod().IsVirtual)
                                            .ToArray();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.HasAttribute<NotMappedAttribute>() ||
                    propertyInfo.HasAttribute<JsonIgnoreAttribute>() ||
                    propertyInfo.HasAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() ||
                    IgnoreFieldNames.Contains(propertyInfo.Name))
                    continue;

                var        entityParam = Expression.Parameter(typeof(TEntity), "_");
                Expression columnExpr  = Expression.Property(entityParam, propertyInfo);

                if (propertyInfo.PropertyType.IsValueType)
                {
                    columnExpr = Expression.Convert(columnExpr, typeof(object));
                }

                var expr = ExpressionExtensions.BuildMemberAccessExpression<TEntity>(propertyInfo.Name);

                if (propertyInfo.Name == "Id" || propertyInfo.HasAttribute<KeyAttribute>())
                {
                    Field(propertyInfo, expr, type: typeof(IdGraphType));
                }
                else
                {
                    var isNullable = propertyInfo.PropertyType.IsNullable() ||
                                     propertyInfo.PropertyType == typeof(string);
                    Field(propertyInfo, expr, nullable: isNullable);
                }
            }

            var methods = GetType()
                         .GetMethods()
                         .Where(_ => _.HasAttribute<GraphQLPropertyAttribute>());

            foreach (var method in methods)
            {
                var propertyAttribute = method.GetCustomAttribute<GraphQLPropertyAttribute>();
                var isAwaitable       = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

                if (isAwaitable)
                {
                    FieldAsync(propertyAttribute.ReturnType,
                               propertyAttribute.Name,
                               method.Description(),
                               resolve: context => ResolveMethod(method, context),
                               deprecationReason: method.ObsoleteMessage());
                }
                else
                {
                    Field(type: propertyAttribute.ReturnType,
                          name: propertyAttribute.Name,
                          description: method.Description(),
                          resolve: context => ResolveMethod(method, context).Result,
                          deprecationReason: method.ObsoleteMessage());
                }
            }
        }

        private async Task<object> ResolveMethod(MethodInfo method, IResolveFieldContext<TEntity> context)
        {
            var isAwaitable = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

            if (isAwaitable)
            {
                return await (dynamic) method.Invoke(this, new object[] {context});
            }

            return method.Invoke(this, new object[] {context});
        }

        private void Field(PropertyInfo                      property,
                           Expression<Func<TEntity, object>> expression,
                           bool                              nullable = false,
                           Type                              type     = null)
        {
            var propertyType = property.PropertyType;

            try
            {
                if (type == null)
                    type = propertyType.GetGraphTypeFromType(nullable);
            }
            catch (ArgumentOutOfRangeException exp)
            {
                throw new ArgumentException(
                                            $"The GraphQL type for Field: '{property.Name}' on parent type: '{Name ?? GetType().Name}' could not be derived implicitly. \n",
                                            exp
                                           );
            }

            var fieldResolverType = typeof(DynamicExpressionFieldResolver<,>).MakeGenericType(typeof(TEntity), propertyType);

            var fieldResolver = Activator.CreateInstance(fieldResolverType, expression) as IFieldResolver;

            var fieldType = new EventStreamFieldType
            {
                Name              = property.Name,
                Type              = type,
                Arguments         = new QueryArguments(),
                Resolver          = fieldResolver,
                Description       = expression.DescriptionOf() ?? property.Description(),
                DeprecationReason = expression.DeprecationReasonOf() ?? property.ObsoleteMessage()
            };

            if (expression.Body is MemberExpression expr)
            {
                fieldType.Metadata[ORIGINAL_EXPRESSION_PROPERTY_NAME] = expr.Member.Name;
            }

            AddField(fieldType);
        }
    }
}
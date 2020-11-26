using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;

namespace DotNetBrightener.Integration.GraphQL
{
    /// <summary>
    ///     Represents the type for GraphQL that provides the mutation purposes
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class BaseGraphQLInputModel<TEntity> : InputObjectGraphType<TEntity> where TEntity: class
    {
        private const string ORIGINAL_EXPRESSION_PROPERTY_NAME = nameof(ORIGINAL_EXPRESSION_PROPERTY_NAME);

        private static readonly string[] DefaultIgnoredFields =
        {
            "Created",
            "CreatedBy",
            "LastUpdated",
            "LastUpdatedBy",
            "IsDeleted",
            "Deleted",
        };

        protected virtual string[] IgnoreFieldNames { get; } = { };

        protected BaseGraphQLInputModel()
        {
            Description       = this.GetType().Description();
            DeprecationReason = this.GetType().ObsoleteMessage();

            var properties = typeof(TEntity).GetProperties()
                                            .Where(_ => !_.GetSetMethod().IsVirtual)
                                            .ToArray();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.Name == "Id" ||
                    propertyInfo.HasAttribute<NotMappedAttribute>() ||
                    propertyInfo.HasAttribute<KeyAttribute>() ||
                    propertyInfo.HasAttribute<JsonIgnoreAttribute>() ||
                    propertyInfo.HasAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() ||
                    IgnoreFieldNames.Contains(propertyInfo.Name) ||
                    DefaultIgnoredFields.Contains(propertyInfo.Name))
                    continue;

                var        entityParam = Expression.Parameter(typeof(TEntity), "_");
                Expression columnExpr  = Expression.Property(entityParam, propertyInfo);

                if (propertyInfo.PropertyType.IsValueType)
                {
                    columnExpr = Expression.Convert(columnExpr, typeof(object));
                }

                var expr = ExpressionExtensions.BuildMemberAccessExpression<TEntity>(propertyInfo.Name);

                var isNullable = propertyInfo.PropertyType.IsNullable() ||
                                 propertyInfo.PropertyType == typeof(string);
                Field(propertyInfo, expr, nullable: isNullable);
            }
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

            var fieldResolverType =
                typeof(DynamicExpressionFieldResolver<,>).MakeGenericType(typeof(TEntity), propertyType);

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
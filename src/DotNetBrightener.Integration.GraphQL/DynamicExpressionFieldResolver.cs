using System;
using System.Linq.Expressions;
using GraphQL;
using GraphQL.Resolvers;

namespace DotNetBrightener.Integration.GraphQL
{
    internal class DynamicExpressionFieldResolver<TSourceType, TProperty> : IFieldResolver<TProperty>
    {
        private readonly Func<TSourceType, object> _property;

        public DynamicExpressionFieldResolver(Expression<Func<TSourceType, object>> property)
        {
            _property = property.Compile();
        }

        public TProperty Resolve(IResolveFieldContext context)
        {
            if (_property((TSourceType) context.Source) is TProperty propertyValue)
            {
                return propertyValue;
            }

            return default;
        }

        object IFieldResolver.Resolve(IResolveFieldContext context) => Resolve(context);
    }
}
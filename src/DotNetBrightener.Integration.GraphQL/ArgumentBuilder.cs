using System;
using GraphQL.Types;

namespace DotNetBrightener.Integration.GraphQL
{
    public class ArgumentBuilder
    {
        private readonly QueryArguments _queryArguments = new QueryArguments();

        public static ArgumentBuilder New()
        {
            return new ArgumentBuilder();
        }

        public ArgumentBuilder AddArgument<TArgumentType>(string name) where TArgumentType : IGraphType
        {
            _queryArguments.Add(new QueryArgument<TArgumentType>
            {
                Name = name
            });

            return this;
        }

        public ArgumentBuilder AddArgument(string name, Type argumentType)
        {
            if (!typeof(IGraphType).IsAssignableFrom(argumentType))
            {
                throw new ArgumentException($"Argument Type must derive from IGraphType");
            }

            _queryArguments.Add(new QueryArgument(argumentType)
            {
                Name = name
            });

            return this;
        }

        public QueryArguments Finalize()
        {
            return _queryArguments;
        }
    }
}
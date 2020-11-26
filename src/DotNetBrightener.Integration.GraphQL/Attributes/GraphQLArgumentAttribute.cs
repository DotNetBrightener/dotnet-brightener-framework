using System;
using GraphQL.Types;

namespace DotNetBrightener.Integration.GraphQL.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class GraphQLArgumentAttribute : Attribute
    {
        internal Type ArgumentType { get; set; }

        internal string ArgumentName { get; set; }

        public GraphQLArgumentAttribute(string name, Type argumentType, bool nullable = true)
        {
            ArgumentName = name;

            if (nullable)
            {
                ArgumentType = argumentType;
            }
            else
            {
                ArgumentType = typeof(NonNullGraphType<>).MakeGenericType(argumentType);
            }
        }
    }
}
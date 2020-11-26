using System;

namespace DotNetBrightener.Integration.GraphQL.Attributes
{
    public class GraphQLPropertyAttribute : Attribute
    {
        internal string Name { get; set; }

        internal Type ReturnType { get; set; }

        public GraphQLPropertyAttribute()
        {
        }

        public GraphQLPropertyAttribute(string name, Type returnType)
        {
            Name       = name;
            ReturnType = returnType;
        }
    }
}
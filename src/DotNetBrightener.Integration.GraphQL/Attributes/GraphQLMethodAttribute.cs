using System;

namespace DotNetBrightener.Integration.GraphQL.Attributes
{
    public class GraphQLMethodAttribute : Attribute
    {
        internal string Name { get; set; }

        internal Type ReturnType { get; set; }

        public GraphQLMethodAttribute()
        {
        }

        public GraphQLMethodAttribute(string name, Type returnType = null)
        {
            Name = name;
            if (returnType != null)
                ReturnType = returnType;
        }
    }
}
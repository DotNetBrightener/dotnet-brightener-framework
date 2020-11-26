using System;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Integration.GraphQL
{
    public class DnbAppSchema : Schema
    {
        public DnbAppSchema(IServiceProvider resolver) : base(resolver)
        {
            Query = resolver.GetService<AppQuery>();
            Mutation = resolver.GetService<AppMutation>();
        }
    }
}
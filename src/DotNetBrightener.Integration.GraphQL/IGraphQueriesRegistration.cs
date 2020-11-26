using System.Collections.Generic;
using GraphQL.Types;

namespace DotNetBrightener.Integration.GraphQL
{
    public interface IGraphQueriesRegistration
    {
        IEnumerable<FieldType> Fields { get; }
    }
}
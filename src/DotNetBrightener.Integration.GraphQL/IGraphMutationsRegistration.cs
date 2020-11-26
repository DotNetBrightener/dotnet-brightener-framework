using System.Collections.Generic;
using GraphQL.Types;

namespace DotNetBrightener.Integration.GraphQL
{
    public interface IGraphMutationsRegistration
    {
        IEnumerable<FieldType> Fields { get; }
    }
}
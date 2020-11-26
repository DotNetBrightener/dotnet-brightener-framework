using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace DotNetBrightener.Integration.GraphQL
{
    public class AppMutation : ObjectGraphType
    {
        public AppMutation(IEnumerable<IGraphMutationsRegistration> graphTypeRegistrations)
        {
            var typeRegistrations = graphTypeRegistrations.ToList();

            var allFields = typeRegistrations.SelectMany(_ => _.Fields)
                                             .ToList();
            
            foreach (var fieldType in allFields)
            {
                if (!HasField(fieldType.Name))
                    AddField(fieldType);
            }
        }
    }
}
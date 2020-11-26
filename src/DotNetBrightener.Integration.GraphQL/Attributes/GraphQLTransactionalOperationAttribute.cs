using System;

namespace DotNetBrightener.Integration.GraphQL.Attributes
{
    /// <summary>
    ///     Marks the method as a transactional operation.
    ///     If an exception is thrown from the execution, the transaction will be rolled back automatically
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class GraphQLTransactionalOperationAttribute: Attribute { }
}
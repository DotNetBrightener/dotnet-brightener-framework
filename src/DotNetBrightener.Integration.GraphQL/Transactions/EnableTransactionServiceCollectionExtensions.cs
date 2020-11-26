using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Integration.GraphQL.Transactions
{
    public static class EnableTransactionServiceCollectionExtensions
    {
        public static IServiceCollection EnableDataTransaction(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ITransactionManager, TransactionManager>();

            return serviceCollection;
        }
    }
}
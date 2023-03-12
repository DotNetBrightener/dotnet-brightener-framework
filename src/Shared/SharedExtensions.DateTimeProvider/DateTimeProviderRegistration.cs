namespace Microsoft.Extensions.DependencyInjection.Extensions;

internal static class DateTimeProviderRegistration
{
    public static void AddSystemDateTimeProvider(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
    }
}
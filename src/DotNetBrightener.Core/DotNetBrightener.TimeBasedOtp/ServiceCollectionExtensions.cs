using DotNetBrightener.TimeBaseOtp;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddOtpProvider(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        serviceCollection.AddScoped<IOTPProvider, TimeBasedOTPProvider>();
    }
}

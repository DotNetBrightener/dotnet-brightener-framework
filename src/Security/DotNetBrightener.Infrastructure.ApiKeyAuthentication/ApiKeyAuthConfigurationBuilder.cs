using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication;

public class ApiKeyAuthConfigurationBuilder
{
    internal IServiceCollection ServiceCollection { get; set; }
}
using Microsoft.Extensions.Configuration;

namespace WebApp.CommonShared.Internal;

internal static class GetDefaultAllowedOriginsFromConfigurationExtension
{
    private static string[] _defaultAllowedOrigins = [];
    private static bool     _isInitialized         = false;

    internal static string[] GetDefaultAllowedOrigins(this IConfiguration configuration)
    {
        if (_isInitialized)
            return _defaultAllowedOrigins;

        var allowedOriginConfigs = configuration.GetValue<string>("ASPNETCORE__AllowedCorsOrigins") ??
                                   Environment.GetEnvironmentVariable("ASPNETCORE__AllowedCorsOrigins");

        if (string.IsNullOrEmpty(allowedOriginConfigs))
            return [];

        _defaultAllowedOrigins =
            allowedOriginConfigs.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Distinct()
                                .ToArray();

        _isInitialized = true;

        return _defaultAllowedOrigins;
    }
}
// ReSharper disable CheckNamespace

using DotNetBrightener.Core.Localization.Factories;
using DotNetBrightener.Core.Localization.Middlewares;
using DotNetBrightener.Core.Localization.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;

namespace Microsoft.Extensions.DependencyInjection;

public static class LocalizationModuleExtensions
{
    /// <summary>
    ///     Adds the localization services using Json files as dictionary to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">
    ///     The service collection
    /// </param>
    public static IServiceCollection AddDotNetBrightenerLocalization(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IStringLocalizerFactory, JsonBasedStringLocalizerFactory>();
        serviceCollection.AddSingleton<JsonDictionaryBasedStringLocalizer>();

        serviceCollection.AddSingleton<ILocalizationManager, LocalizationManager>();
        serviceCollection.AddSingleton<ILocalizationFileManager, DefaultLocalizationFileManager>();

        serviceCollection.AddLocalization();

        return serviceCollection;
    }

    /// <summary>
    ///     Register <see cref="requestPath"/> as the endpoint of retrieving localization dictionary
    /// </summary>
    /// <param name="app"></param>
    /// <param name="requestPath">
    ///     The request endpoint to retrieve localization dictionary
    /// </param>
    public static IApplicationBuilder UseTranslationDictionaryEndpoint(this IApplicationBuilder app, string requestPath)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<TranslationDictionaryMiddleware>(requestPath);
    }
}
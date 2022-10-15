using System;
using DotNetBrightener.Core.Localization.Factories;
using DotNetBrightener.Core.Localization.Middlewares;
using DotNetBrightener.Core.Localization.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.Core.Localization.Extensions;

public static class LocalizationModuleExtensions
{
    public static IServiceCollection AddDotNetBrightenerLocalization(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IStringLocalizerFactory, JsonBasedStringLocalizerFactory>();
        serviceCollection.AddSingleton<JsonDictionaryBasedStringLocalizer>();

        serviceCollection.AddSingleton<ILocalizationManager, LocalizationManager>();
        serviceCollection.AddSingleton<ILocalizationFileManager, DefaultLocalizationFileManager>();

        serviceCollection.AddLocalization();

        return serviceCollection;
    }

    public static IApplicationBuilder UseTranslationDictionary(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<TranslationDictionaryMiddleware>();
    }
}
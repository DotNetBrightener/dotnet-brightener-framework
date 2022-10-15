using System;
using DotNetBrightener.Mvc.HandlebarsViewEngine.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine;

/// <summary>
///     Adds the extension setup for <see cref="MvcViewOptions"/> to extend the view engines
/// </summary>
public class HandleBarMvcViewOptionsSetup : IConfigureOptions<MvcViewOptions>
{
    private readonly IHandlebarsViewEngine _handleBarViewEngine;

    public HandleBarMvcViewOptionsSetup(IHandlebarsViewEngine handleBarViewEngine)
    {
        _handleBarViewEngine = handleBarViewEngine ?? throw new ArgumentNullException(nameof(handleBarViewEngine));
    }

    public void Configure(MvcViewOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.ViewEngines.Add(_handleBarViewEngine);
    }
}
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Mvc.HandlebarsViewEngine;

public class HandleBarViewOptionsSetup : ConfigureOptions<HandlebarsMvcViewOptions>
{
    public HandleBarViewOptionsSetup() : base(Configure) { }

    public new static void Configure(HandlebarsMvcViewOptions options)
    {
        options.ViewLocationFormats.Add("/Modules/{2}/Areas/{3}/Views/{1}/{0}.html");
        options.ViewLocationFormats.Add("/Modules/{2}/Areas/{3}/Views/Shared/{0}.html");
        options.ViewLocationFormats.Add("/Modules/{2}/Views/{1}/{0}.html");
        options.ViewLocationFormats.Add("/Modules/{2}/Views/Shared/{0}.html");
        options.ViewLocationFormats.Add("/Areas/{3}/Views/{1}/{0}.html");
        options.ViewLocationFormats.Add("/Areas/{3}/Views/Shared/{0}.html");
        options.ViewLocationFormats.Add("/Views/{1}/{0}.html");
        options.ViewLocationFormats.Add("/Views/Shared/{0}.html");
        options.ViewLocationFormats.Add("/Views/{1}/{0}.html");
        options.ViewLocationFormats.Add("/Views/Shared/{0}.html");
    }
}
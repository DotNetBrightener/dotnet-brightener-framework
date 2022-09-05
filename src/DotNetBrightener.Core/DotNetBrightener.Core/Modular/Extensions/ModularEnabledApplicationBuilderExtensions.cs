using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotNetBrightener.Core.Exceptions;
using DotNetBrightener.Core.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.Core.Modular.Extensions;

public static class ModularEnabledApplicationBuilderExtensions
{
    /// <summary>
    ///     Enables modular static file servers to the modules
    /// </summary>
    /// <param name="app"></param>
    public static void UseModularStaticFileServers(this IApplicationBuilder app)
    {
        var serviceProvider = app.ApplicationServices;
        var modules = serviceProvider.GetService<LoadedModuleEntries>()
                                     .ToList();
        var environment = serviceProvider.GetService<IWebHostEnvironment>();

        foreach (var module in modules)
        {
            var requestPath = module.ModuleId;

            if (requestPath == ModuleEntry.MainModuleIdentifier)
            {
                requestPath = string.Empty;
                module.UseStaticFileProvider(environment.WebRootFileProvider);
            }

            requestPath = !string.IsNullOrEmpty(requestPath) ? "/" + requestPath : requestPath;
            EnableStaticFileServer(app, module.StaticFileProvider, requestPath);

            if (!string.IsNullOrEmpty(module.Alias))
            {
                EnableStaticFileServer(app, module.StaticFileProvider, $"/{module.Alias}");
            }
        }

        app.UseStatusCodePages(context => HandleModuleSpaRequests(context, modules));
    }

    private static void EnableStaticFileServer(IApplicationBuilder app, IFileProvider moduleFileProvider, string requestPath)
    {
        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = moduleFileProvider,
            RequestPath  = new PathString(requestPath),
        };

        var fileServerOptions = new FileServerOptions
        {
            FileProvider            = moduleFileProvider,
            RequestPath             = new PathString(requestPath),
            EnableDirectoryBrowsing = false
        };

        staticFileOptions.OnPrepareResponse =
            fileServerOptions.StaticFileOptions.OnPrepareResponse =
                context => OnPrepareResponse(context, requestPath);

        app.UseStaticFiles(staticFileOptions);
        app.UseFileServer(fileServerOptions);
    }

    private static void OnPrepareResponse(StaticFileResponseContext context, string requestPath)
    {
        if (context.Context.Request.Path.Equals($"{requestPath}/sw.js"))
        {
            HandleServiceWorkerRequest(context, requestPath);
        }
    }

    private static void HandleServiceWorkerRequest(StaticFileResponseContext context, string requestPath)
    {
        var refererHeader =
            context.Context.Request.Headers.FirstOrDefault(_ => _.Key.ToLower().Equals("referer"));

        var referrerUrl = new Uri(refererHeader.Value.ToString());
        var path        = new PathString(referrerUrl.AbsolutePath);

        if (path.StartsWithSegments($"{requestPath}"))
        {
            context.Context.Response.Headers.Add("Service-Worker-Allowed", $"{requestPath}");
        }
    }

    private static async Task HandleModuleSpaRequests(StatusCodeContext context,
                                                      List<ModuleEntry> moduleEntries)
    {
        if (context.HttpContext.Response.StatusCode != 404)
            return;

        var requestPath = context.HttpContext.Request.Path;
        var moduleEntry =
            moduleEntries.FirstOrDefault(_ => !string.IsNullOrEmpty(_.Alias) &&
                                              requestPath.StartsWithSegments($"/{_.Alias}") ||
                                              requestPath.StartsWithSegments($"/{_.ModuleId}"));

        if (moduleEntry == null || !moduleEntry.EnableSpa)
        {
            ThrowNotFoundException(context);
            return;
        }

        var spaModuleHandler = context.HttpContext.RequestServices.GetService<ISpaModuleHandler>();

        if (spaModuleHandler == null)
        {
            ThrowNotFoundException(context);
            return;
        }

        await spaModuleHandler.ProcessSpaRequest(context.HttpContext.Response, moduleEntry.StaticFileProvider);
    }

    private static void ThrowNotFoundException(StatusCodeContext context)
    {
        var localizer         = context.HttpContext.RequestServices.GetService<IStringLocalizer<HttpContext>>();
        var exceptionHandlers = context.HttpContext.RequestServices.GetServices<IUnhandleExceptionHandler>();

        var exception = new BadHttpRequestException(localizer ["ErrorMessages.TheRequestedResourceCannotBeFound"], (int) HttpStatusCode.NotFound);

        var exceptionContext = new UnhandledExceptionContext
        {
            ContextException = exception,
            StatusCode       = (HttpStatusCode) exception.StatusCode
        };

        foreach(var handler in exceptionHandlers)
        {
            handler.HandleException(exceptionContext);
            if (exceptionContext.ProcessResult != null)
                break;
        }

        if (exceptionContext.ProcessResult != null && exceptionContext.ProcessResult is ContentResult contentResult)
        {
            context.HttpContext.Response.StatusCode  = contentResult.StatusCode.Value;
            context.HttpContext.Response.ContentType = contentResult.ContentType;
            context.HttpContext.Response.WriteAsync(contentResult.Content);
            return;
        }

        throw exception;
    }
}
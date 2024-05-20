using DotNetBrightener.Utils.MessageCompression;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DotNetBrightener.SecuredApi;

internal class SecuredApiOptions
{
    public string BasePath { get; set; }
}

internal class SecureApiHandleMiddleware
{
    private readonly RequestDelegate   _next;
    private readonly ILogger           _logger;
    private readonly SecuredApiOptions _options;

    public SecureApiHandleMiddleware(SecuredApiOptions                  options,
                                     RequestDelegate                    next,
                                     ILogger<SecureApiHandleMiddleware> logger)
    {
        options  ??= new SecuredApiOptions();
        _options =   options;
        _next    =   next;
        _logger  =   logger;
    }

    public async Task InvokeAsync(HttpContext             context,
                                  IServiceProvider        serviceProvider,
                                  SecuredApiHandlerRouter handlerRouter)
    {
        if (!string.IsNullOrEmpty(_options.BasePath) &&
            !context.Request.Path.StartsWithSegments(_options.BasePath))
        {
            await _next(context);

            return;
        }

        // load the last segment from context.Request.Path
        var action = context.Request.Path.Value;

        if (!string.IsNullOrEmpty(_options.BasePath))
        {
            action = action!.Substring(_options.BasePath.Length);
        }

        if (string.IsNullOrEmpty(action))
        {
            await _next(context);

            return;
        }

        action = action.Trim().TrimStart('/');

        if (!handlerRouter.TryGetValue(action, out var apiHandlerMetadata) ||
            apiHandlerMetadata.HttpMethod.Method != context.Request.Method ||
            serviceProvider.TryGet(apiHandlerMetadata.HandlerType) is not IApiHandler handler ||
            handler is not BaseApiHandler baseHandler)
        {
            await _next(context);

            return;
        }

        baseHandler.HttpContext = context;
        baseHandler.Request     = context.Request;
        baseHandler.Response    = context.Response;

        try
        {
            // read the request body from the binary
            var bytesArrayFromBody = context.Request.BodyReader.AsStream();

            ApiMessage apiMessage;

            await using (var memStream = new MemoryStream())
            {
                await bytesArrayFromBody.CopyToAsync(memStream);
                apiMessage = await memStream.Decompress<ApiMessage>(1024 * 4);
            }

            var processedResult = await handler.ProcessMessage(apiMessage!);

            if (processedResult != null)
            {
                if (processedResult.ShortCircuit)
                {
                    return;
                }

                var jsonResponseBytes = await processedResult.ToJsonBytes();

                var responseBody = await jsonResponseBytes.Compress();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.Body.WriteAsync(responseBody);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                             "Error processing secured API request: {requestUrl}",
                             context.Request.GetDisplayUrl());
            context.Response.StatusCode = 500;
        }
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebApp.CommonShared.Endpoints.Validation;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
///     Extension methods for creating routes with automatic model validation.
/// </summary>
public static class ValidatedRouteExtensions
{
    /// <summary>
    ///     Maps a POST endpoint with automatic model validation.
    ///     Returns 400 Bad Request with validation errors if validation fails.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request model to validate</typeparam>
    /// <param name="endpoints">The endpoint route builder</param>
    /// <param name="pattern">The route pattern</param>
    /// <param name="handler">The route handler that receives the validated model</param>
    /// <returns>A route handler builder for further configuration</returns>
    /// <example>
    /// app.MapPostWithValidation&lt;CreateUserRequest&gt;("/users", async (request, context) =>
    /// {
    ///     // request is already validated
    ///     var user = await userService.CreateAsync(request);
    ///     return Results.Created($"/users/{user.Id}", user);
    /// });
    /// </example>
    public static RouteHandlerBuilder MapPostWithValidation<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<TRequest, HttpContext, Task<IResult>> handler)
    {
        return endpoints.MapPost(pattern, async (TRequest model, HttpContext context) =>
        {
            var validationResult = context.ValidateAndGetResult(model);

            if (validationResult != null)
            {
                return validationResult;
            }

            return await handler(model, context);
        });
    }

    /// <summary>
    ///     Maps a POST endpoint with automatic model validation (with cancellation support).
    /// </summary>
    public static RouteHandlerBuilder MapPostWithValidation<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<TRequest, HttpContext, CancellationToken, Task<IResult>> handler)
    {
        return endpoints.MapPost(pattern, async (TRequest model, HttpContext context, CancellationToken ct) =>
        {
            var validationResult = await context.ValidateAndGetResultAsync(model, ct);

            if (validationResult != null)
            {
                return validationResult;
            }

            return await handler(model, context, ct);
        });
    }

    /// <summary>
    ///     Maps a PUT endpoint with automatic model validation.
    ///     Returns 400 Bad Request with validation errors if validation fails.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request model to validate</typeparam>
    /// <param name="endpoints">The endpoint route builder</param>
    /// <param name="pattern">The route pattern</param>
    /// <param name="handler">The route handler that receives the validated model</param>
    /// <returns>A route handler builder for further configuration</returns>
    public static RouteHandlerBuilder MapPutWithValidation<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<TRequest, HttpContext, Task<IResult>> handler)
    {
        return endpoints.MapPut(pattern, async (TRequest model, HttpContext context) =>
        {
            var validationResult = context.ValidateAndGetResult(model);

            if (validationResult != null)
            {
                return validationResult;
            }

            return await handler(model, context);
        });
    }

    /// <summary>
    ///     Maps a PUT endpoint with automatic model validation (with cancellation support).
    /// </summary>
    public static RouteHandlerBuilder MapPutWithValidation<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<TRequest, HttpContext, CancellationToken, Task<IResult>> handler)
    {
        return endpoints.MapPut(pattern, async (TRequest model, HttpContext context, CancellationToken ct) =>
        {
            var validationResult = await context.ValidateAndGetResultAsync(model, ct);

            if (validationResult != null)
            {
                return validationResult;
            }

            return await handler(model, context, ct);
        });
    }

    /// <summary>
    ///     Maps a PATCH endpoint with automatic model validation.
    ///     Returns 400 Bad Request with validation errors if validation fails.
    /// </summary>
    public static RouteHandlerBuilder MapPatchWithValidation<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<TRequest, HttpContext, Task<IResult>> handler)
    {
        return endpoints.MapPatch(pattern, async (TRequest model, HttpContext context) =>
        {
            var validationResult = context.ValidateAndGetResult(model);

            if (validationResult != null)
            {
                return validationResult;
            }

            return await handler(model, context);
        });
    }

    /// <summary>
    ///     Maps a PATCH endpoint with automatic model validation (with cancellation support).
    /// </summary>
    public static RouteHandlerBuilder MapPatchWithValidation<TRequest>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<TRequest, HttpContext, CancellationToken, Task<IResult>> handler)
    {
        return endpoints.MapPatch(pattern, async (TRequest model, HttpContext context, CancellationToken ct) =>
        {
            var validationResult = await context.ValidateAndGetResultAsync(model, ct);

            if (validationResult != null)
            {
                return validationResult;
            }

            return await handler(model, context, ct);
        });
    }
}

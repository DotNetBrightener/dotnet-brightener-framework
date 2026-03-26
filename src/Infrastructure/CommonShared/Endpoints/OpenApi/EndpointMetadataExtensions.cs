using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
///     Extension methods for adding OpenAPI metadata to endpoints.
///     Provides a fluent API for documenting endpoints in Swagger/OpenAPI.
/// </summary>
public static class EndpointMetadataExtensions
{
    /// <summary>
    ///     Adds OpenAPI metadata with name, summary, and optional description.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="name">The operation name (used as operationId)</param>
    /// <param name="summary">A brief summary of what the endpoint does</param>
    /// <param name="description">Optional detailed description</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// app.MapGet("/users", GetUsers)
    ///     .WithOpenApiInfo("GetUsers", "Gets all users", "Returns a paginated list of users");
    /// </example>
    public static RouteHandlerBuilder WithOpenApiInfo(
        this RouteHandlerBuilder builder,
        string name,
        string summary,
        string? description = null)
    {
        builder.WithName(name);
        builder.WithDescription(description ?? summary);

        return builder;
    }

    /// <summary>
    ///     Adds tags to the endpoint for OpenAPI grouping.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="tags">The tags to apply</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// app.MapGet("/users", GetUsers)
    ///     .WithEndpointTags("Users", "Administration");
    /// </example>
    public static RouteHandlerBuilder WithEndpointTags(
        this RouteHandlerBuilder builder,
        params string[] tags)
    {
        builder.WithTags(tags);

        return builder;
    }

    /// <summary>
    ///     Marks the endpoint to be included in OpenAPI documentation.
    ///     By default, this adds a visible tag for grouping.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="tag">Optional tag for grouping. If not provided, no tag is added.</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// app.MapGet("/users", GetUsers)
    ///     .IncludeInOpenApi("Users");
    /// </example>
    public static RouteHandlerBuilder IncludeInOpenApi(
        this RouteHandlerBuilder builder,
        string? tag = null)
    {
        if (!string.IsNullOrEmpty(tag))
        {
            builder.WithTags(tag);
        }

        return builder;
    }

    /// <summary>
    ///     Adds response type metadata for successful responses.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="builder">The route handler builder</param>
    /// <param name="statusCode">The HTTP status code (default 200)</param>
    /// <param name="contentType">The content type (default application/json)</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// app.MapGet("/users/{id}", GetUser)
    ///     .ProducesResponse&lt;UserDto&gt;(200);
    /// </example>
    public static RouteHandlerBuilder ProducesResponse<TResponse>(
        this RouteHandlerBuilder builder,
        int statusCode = 200,
        string contentType = "application/json")
    {
        builder.Produces<TResponse>(statusCode, contentType);

        return builder;
    }

    /// <summary>
    ///     Adds response type metadata for error responses.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="contentType">The content type</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// app.MapGet("/users/{id}", GetUser)
    ///     .ProducesError(404);
    /// </example>
    public static RouteHandlerBuilder ProducesError(
        this RouteHandlerBuilder builder,
        int statusCode,
        string contentType = "application/problem+json")
    {
        builder.ProducesProblem(statusCode, contentType);

        return builder;
    }

    /// <summary>
    ///     Adds common error responses (400, 401, 403, 404, 500) to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="includeNotFound">Whether to include 404 response</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// app.MapGet("/users/{id}", GetUser)
    ///     .WithCommonErrorResponses();
    /// </example>
    public static RouteHandlerBuilder WithCommonErrorResponses(
        this RouteHandlerBuilder builder,
        bool includeNotFound = true)
    {
        builder.ProducesValidationProblem();

        if (includeNotFound)
        {
            builder.ProducesProblem(404);
        }

        builder.ProducesProblem(500);

        return builder;
    }
}

using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.CommonShared.Endpoints.Validation;

/// <summary>
///     Extension methods for model validation using FluentValidation.
/// </summary>
public static class ModelValidationExtensions
{
    /// <summary>
    ///     Validates the model using registered FluentValidation validators.
    /// </summary>
    /// <typeparam name="T">The type of model to validate</typeparam>
    /// <param name="context">The HTTP context to retrieve validators from</param>
    /// <param name="model">The model instance to validate</param>
    /// <returns>The validation result</returns>
    /// <example>
    /// var result = context.Validate(request);
    /// if (!result.IsValid)
    /// {
    ///     return Results.ValidationProblem(result.ToDictionary());
    /// }
    /// </example>
    public static ValidationResult Validate<T>(this HttpContext context, T model)
    {
        var validator = context.RequestServices.GetService<IValidator<T>>();

        return validator?.Validate(model) ?? new ValidationResult();
    }

    /// <summary>
    ///     Validates the model asynchronously using registered FluentValidation validators.
    /// </summary>
    /// <typeparam name="T">The type of model to validate</typeparam>
    /// <param name="context">The HTTP context to retrieve validators from</param>
    /// <param name="model">The model instance to validate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The validation result</returns>
    public static async Task<ValidationResult> ValidateAsync<T>(
        this HttpContext context,
        T model,
        CancellationToken cancellationToken = default)
    {
        var validator = context.RequestServices.GetService<IValidator<T>>();

        return validator is null
            ? new ValidationResult()
            : await validator.ValidateAsync(model, cancellationToken);
    }

    /// <summary>
    ///     Validates the model and returns a validation problem result if invalid.
    ///     Returns null if validation passes.
    /// </summary>
    /// <typeparam name="T">The type of model to validate</typeparam>
    /// <param name="context">The HTTP context</param>
    /// <param name="model">The model to validate</param>
    /// <returns>
    ///     A <see cref="IResult"/> with validation problems if invalid, null if valid.
    /// </returns>
    /// <example>
    /// var validation = context.ValidateAndGetResult(request);
    /// if (validation != null) return validation;
    /// </example>
    public static IResult? ValidateAndGetResult<T>(this HttpContext context, T model)
    {
        var result = context.Validate(model);

        return result.IsValid ? null : Results.ValidationProblem(result.ToDictionary());
    }

    /// <summary>
    ///     Validates the model asynchronously and returns a validation problem result if invalid.
    ///     Returns null if validation passes.
    /// </summary>
    /// <typeparam name="T">The type of model to validate</typeparam>
    /// <param name="context">The HTTP context</param>
    /// <param name="model">The model to validate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>
    ///     A <see cref="IResult"/> with validation problems if invalid, null if valid.
    /// </returns>
    public static async Task<IResult?> ValidateAndGetResultAsync<T>(
        this HttpContext context,
        T model,
        CancellationToken cancellationToken = default)
    {
        var result = await context.ValidateAsync(model, cancellationToken);

        return result.IsValid ? null : Results.ValidationProblem(result.ToDictionary());
    }

    /// <summary>
    ///     Converts validation errors to a dictionary format suitable for problem details.
    /// </summary>
    /// <param name="result">The validation result</param>
    /// <returns>A dictionary of field names to error messages</returns>
    public static Dictionary<string, string[]> ToDictionary(this ValidationResult result)
    {
        return result.Errors
                      .GroupBy(e => e.PropertyName)
                      .ToDictionary(
                           g => g.Key,
                           g => g.Select(e => e.ErrorMessage).ToArray()
                          );
    }
}

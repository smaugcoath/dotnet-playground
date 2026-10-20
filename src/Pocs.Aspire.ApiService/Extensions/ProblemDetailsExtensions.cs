using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pocs.Aspire.Domain.Errors;
using System;
using System.Linq;

namespace Pocs.Aspire.ApiService.Extensions;
/// <summary>
/// Provides extension methods to customize ProblemDetails.
/// </summary>
internal static class ProblemDetailsExtensions
{
    /// <summary>
    /// Converts a ValidationError into a ProblemDetails instance with custom properties.
    /// </summary>
    /// <param name="error">The validation error containing domain errors.</param>
    /// <param name="httpContext">The current HttpContext.</param>
    /// <returns>A ProblemDetails instance representing the validation errors.</returns>
    public static ValidationProblem ToValidationProblem(this ValidationError error, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(httpContext);

        var errors = error.Errors
            .GroupBy(e => e.Field)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());

        return TypedResults.ValidationProblem(
            errors,
            detail: $"{errors.Count} validation error(s) occurred.",
            instance: $"{httpContext.Request.Method} {httpContext.Request.Path}",
            title: "Validation error"
        );
    }

    /// <summary>
    /// Maps a non-validation <see cref="Failure"/> to the problem response it produces.
    /// Validation errors carry field details and go through <see cref="ToValidationProblem"/>.
    /// </summary>
    /// <param name="failure">The failure to map.</param>
    /// <returns>The problem response representing the failure.</returns>
    public static ProblemHttpResult ToProblem(this Failure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);

        return failure switch
        {
            EmailAlreadyExistsError error => TypedResults.Problem(title: error.Message, detail: error.Code, statusCode: StatusCodes.Status409Conflict),
            NotFoundError error => TypedResults.Problem(title: error.Message, detail: error.Code, statusCode: StatusCodes.Status404NotFound),
            UniqueConstraintViolationError error => TypedResults.Problem(title: "A conflicting update occurred.", detail: error.Code, statusCode: StatusCodes.Status409Conflict),
            _ => TypedResults.Problem(title: "Unexpected failure", detail: failure.Code, statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}

using FluentValidation;
using LanguageExt;
using Pocs.Aspire.Domain.Errors;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.Business.Validations;

public static class ValidatorEitherExtensions
{
    /// <summary>
    /// Validates <paramref name="request"/> and lifts the outcome into the use-case pipeline:
    /// the request itself when valid, a <see cref="ValidationError"/> otherwise.
    /// </summary>
    public static async Task<Either<Failure, T>> ValidateToEither<T>(this IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        return validationResult.IsValid
            ? request
            : new ValidationError(validationResult.ToFieldErrors().ToList());
    }
}

using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;

namespace Pocs.Aspire.Business.Users;

internal static class SaveFailureExtensions
{
    internal static Failure ToEmailFailure(this Failure failure, Email email) =>
        failure is UniqueConstraintViolationError { ConstraintName: User.EmailUniqueIndexName }
            ? new EmailAlreadyExistsError(email)
            : failure;
}

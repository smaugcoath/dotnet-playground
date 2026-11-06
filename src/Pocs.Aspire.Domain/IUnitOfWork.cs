using LanguageExt;
using Pocs.Aspire.Domain.Errors;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.Domain;

public interface IUnitOfWork
{
    /// <summary>
    /// Persists the tracked changes, translating expected storage rejections into domain failures:
    /// <see cref="EmailAlreadyExistsError"/> when a saved user's email is already taken,
    /// <see cref="NotFoundError"/> when an updated or deleted entity was removed concurrently, and
    /// <see cref="UniqueConstraintViolationError"/> for any other uniqueness rejection.
    /// Unexpected persistence errors throw.
    /// </summary>
    Task<Either<Failure, Unit>> SaveChangesAsync(CancellationToken cancellationToken = default);
}

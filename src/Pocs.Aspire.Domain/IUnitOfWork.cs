using LanguageExt;
using Pocs.Aspire.Domain.Errors;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.Domain;

public interface IUnitOfWork
{
    /// <summary>
    /// Persists the tracked changes. Returns <see cref="UniqueConstraintViolationError"/>
    /// when the database rejects a unique-index violation; other persistence errors throw.
    /// </summary>
    Task<Either<Failure, Unit>> SaveChangesAsync(CancellationToken cancellationToken = default);
}

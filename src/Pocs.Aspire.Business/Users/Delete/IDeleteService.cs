namespace Pocs.Aspire.Business.Users.Delete;

using LanguageExt;
using Pocs.Aspire.Domain.Errors;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface IDeleteService
{
    Task<Either<Failure, Unit>> DeleteAsync(DeleteRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the request data needed to delete a user.
/// </summary>
/// <param name="Id">The unique identifier of the user to delete.</param>
public record DeleteRequest(Guid Id);

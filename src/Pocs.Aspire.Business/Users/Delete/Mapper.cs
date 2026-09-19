using Pocs.Aspire.Domain.Users.ValueObjects;

namespace Pocs.Aspire.Business.Users.Delete;

/// <summary>
/// Provides extension methods for mapping between User domain entities and request/response DTOs.
/// </summary>
internal static class DeleteMappers
{
    internal static UserId ToDomain(this DeleteRequest request)
        => UserId.From(request.Id);
}

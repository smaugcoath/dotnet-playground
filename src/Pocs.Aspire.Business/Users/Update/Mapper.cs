using Pocs.Aspire.Domain.Users;

namespace Pocs.Aspire.Business.Users.Update;

/// <summary>
/// Provides extension methods for mapping between User domain entities and request/response DTOs.
/// </summary>
internal static class UpdateMappers
{
    internal static UpdateResponse ToResponse(this User user) => new(user.Id, user.FirstName, user.LastName, user.Email);
}

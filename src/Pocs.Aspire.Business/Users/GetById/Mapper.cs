using Pocs.Aspire.Domain.Users;

namespace Pocs.Aspire.Business.Users.GetById;

/// <summary>
/// Provides extension methods for mapping between User domain entities and request/response DTOs.
/// </summary>
internal static class GetByIdMappers
{
    internal static GetByIdResponse ToResponse(this User user)
        => new(user.Id, user.FirstName, user.LastName, user.Email);
}

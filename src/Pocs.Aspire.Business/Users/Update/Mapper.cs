using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;

namespace Pocs.Aspire.Business.Users.Update;

/// <summary>
/// Provides extension methods for mapping between User domain entities and request/response DTOs.
/// </summary>
internal static class UpdateMappers
{
    internal static User ToDomain(this UpdateRequest request) =>
         User.From(UserId.From(request.Id), FirstName.From(request.FirstName), LastName.From(request.LastName), Email.From(request.Email));

    internal static UpdateResponse ToResponse(this User user) => new(user.Id, user.FirstName, user.LastName, user.Email);
}

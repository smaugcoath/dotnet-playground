using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;

namespace Pocs.Aspire.Business.Users.Create;

/// <summary>
/// Provides extension methods for mapping between User domain entities and request/response DTOs.
/// </summary>
internal static class CreateMappers
{
    internal static User ToDomain(this CreateRequest request) =>
         User.New(FirstName.From(request.FirstName), LastName.From(request.LastName), Email.From(request.Email));

    internal static CreateResponse ToResponse(this User user) => new(user.Id);
}

using Pocs.Aspire.Domain.Users;
using System.Collections.Generic;
using System.Linq;

namespace Pocs.Aspire.Business.Users.List;

/// <summary>
/// Provides extension methods for mapping between User domain entities and request/response DTOs.
/// </summary>
internal static class ListMappers
{
    internal static ListItem ToListItem(this User user)
        => new(user.Id, user.FirstName, user.LastName, user.Email);

    internal static ListResponse ToResponse(this ListRequest request, IReadOnlyList<User> users, int totalCount)
        => new(users.Select(u => u.ToListItem()).ToList(), request.Page, request.PageSize, totalCount);
}

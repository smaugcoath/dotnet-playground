namespace Pocs.Aspire.Business.Users.List;

using LanguageExt;
using Pocs.Aspire.Domain.Errors;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IListService
{
    Task<Either<Failure, ListResponse>> ListAsync(ListRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the request data needed to list users, paged.
/// </summary>
/// <param name="Page">The 1-based page number to retrieve.</param>
/// <param name="PageSize">The number of users to retrieve per page.</param>
public record ListRequest(int Page, int PageSize);

/// <summary>
/// Represents one user's summary data within a page of results.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="FirstName">The first name of the user.</param>
/// <param name="LastName">The last name of the user.</param>
/// <param name="Email">The email address of the user.</param>
public record ListItem(Guid Id, string FirstName, string LastName, string Email);

/// <summary>
/// Represents a page of users.
/// </summary>
/// <param name="Items">The users returned for this page.</param>
/// <param name="Page">The 1-based page number returned.</param>
/// <param name="PageSize">The number of users requested per page.</param>
/// <param name="TotalCount">The total number of users across all pages.</param>
public record ListResponse(IReadOnlyList<ListItem> Items, int Page, int PageSize, int TotalCount);

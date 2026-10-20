using Asp.Versioning;
using Asp.Versioning.Builder;
using LanguageExt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Pocs.Aspire.ApiService.Extensions;
using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Business.Users.Delete;
using Pocs.Aspire.Business.Users.GetById;
using Pocs.Aspire.Business.Users.List;
using Pocs.Aspire.Business.Users.Update;
using Pocs.Aspire.Domain.Errors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Endpoints;

internal static class UsersEndpoints
{
    private const string UserCacheTag = "users";

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder builder)
    {
        var apiVersionSet = builder.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = builder.MapGroup("api/v{version:apiVersion}/users")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("Users");

        group.MapPost("", Create)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Creates a user.");
        group.MapPut("{id:guid}", Update)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Updates a user.");
        group.MapGet("{id:guid}", GetById)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(GetById))
            .WithSummary("Retrieves a user by ID.")
            .CacheOutput(policy => policy.Expire(TimeSpan.FromSeconds(5)).SetVaryByRouteValue("id").Tag(UserCacheTag));
        group.MapDelete("{id:guid}", Delete)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Deletes a user.");
        group.MapGet("", List)
            .ProducesValidationProblem()
            .WithSummary("Lists users, paged.");

        return builder;
    }

    public static async Task<Results<CreatedAtRoute<CreateResponse>, ValidationProblem, ProblemHttpResult>> Create(
        CreateRequest request,
        ICreateService createService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await createService.CreateAsync(request, cancellationToken);

        return result.Match<Results<CreatedAtRoute<CreateResponse>, ValidationProblem, ProblemHttpResult>>(
            Right: response => TypedResults.CreatedAtRoute(response, nameof(GetById), new { id = response.Id, version = "1" }),
            Left: failure => failure switch
            {
                ValidationError error => error.ToValidationProblem(httpContext),
                _ => failure.ToProblem()
            });
    }

    public static async Task<Results<Ok<UpdateResponse>, ValidationProblem, ProblemHttpResult>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateRequest request,
        IUpdateService updateService,
        IOutputCacheStore outputCacheStore,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        request = request with { Id = id };

        var result = await updateService.UpdateAsync(request, cancellationToken);
        await result.EvictCachedUsersOnSuccess(outputCacheStore, cancellationToken);

        return result.Match<Results<Ok<UpdateResponse>, ValidationProblem, ProblemHttpResult>>(
            Right: response => TypedResults.Ok(response),
            Left: failure => failure switch
            {
                ValidationError error => error.ToValidationProblem(httpContext),
                _ => failure.ToProblem()
            });
    }

    /// <summary>
    /// Retrieves a user by ID.
    /// </summary>
    public static async Task<Results<Ok<GetByIdResponse>, ValidationProblem, ProblemHttpResult>> GetById(
        [FromRoute] Guid id,
        IGetByIdService getByIdService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        GetByIdRequest request = new(id);
        var result = await getByIdService.GetByIdAsync(request, cancellationToken);

        return result.Match<Results<Ok<GetByIdResponse>, ValidationProblem, ProblemHttpResult>>(
            Right: response => TypedResults.Ok(response),
            Left: failure => failure switch
            {
                ValidationError error => error.ToValidationProblem(httpContext),
                _ => failure.ToProblem()
            });
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    public static async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> Delete(
        [FromRoute] Guid id,
        IDeleteService deleteService,
        IOutputCacheStore outputCacheStore,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        DeleteRequest request = new(id);
        var result = await deleteService.DeleteAsync(request, cancellationToken);
        await result.EvictCachedUsersOnSuccess(outputCacheStore, cancellationToken);

        return result.Match<Results<NoContent, ValidationProblem, ProblemHttpResult>>(
            Right: _ => TypedResults.NoContent(),
            Left: failure => failure switch
            {
                ValidationError error => error.ToValidationProblem(httpContext),
                _ => failure.ToProblem()
            });
    }

    /// <summary>
    /// Lists users, paged.
    /// </summary>
    private static async Task EvictCachedUsersOnSuccess<TResponse>(
        this Either<Failure, TResponse> result,
        IOutputCacheStore outputCacheStore,
        CancellationToken cancellationToken)
    {
        if (result.IsRight)
        {
            await outputCacheStore.EvictByTagAsync(UserCacheTag, cancellationToken);
        }
    }

    public static async Task<Results<Ok<ListResponse>, ValidationProblem, ProblemHttpResult>> List(
        IListService listService,
        HttpContext httpContext,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        ListRequest request = new(page, pageSize);
        var result = await listService.ListAsync(request, cancellationToken);

        return result.Match<Results<Ok<ListResponse>, ValidationProblem, ProblemHttpResult>>(
            Right: response => TypedResults.Ok(response),
            Left: failure => failure switch
            {
                ValidationError error => error.ToValidationProblem(httpContext),
                _ => failure.ToProblem()
            });
    }
}

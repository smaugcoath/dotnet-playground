using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Pocs.Aspire.ApiService.Extensions;
using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Business.Users.GetById;
using Pocs.Aspire.Business.Users.Update;
using Pocs.Aspire.Domain.Errors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Endpoints;

internal static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder builder)
    {
        var apiVersionSet = builder.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = builder.MapGroup("api/v{version:apiVersion}/users")
            .WithApiVersionSet(apiVersionSet);

        group.MapPost("", Create)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("{id:guid}", Update)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapGet("{id:guid}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(GetById))
            .CacheOutput(policy => policy.Expire(TimeSpan.FromSeconds(5)).SetVaryByRouteValue("id"));

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
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        request = request with { Id = id };

        var result = await updateService.UpdateAsync(request, cancellationToken);

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
}

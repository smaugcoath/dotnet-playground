using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Business.Users.GetById;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Tests.Functional.Users;

[Collection(SharedAspireHost.Name)]
public class UserDeleteFunctionalTests
{
    private readonly AspireHostFixture _fixture;

    public UserDeleteFunctionalTests(AspireHostFixture fixture)
    {
        _fixture = fixture;
    }

    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

#pragma warning disable CA1812 // instantiated via JSON deserialization
    private sealed record ValidationErrorsPayload(string? Title, Dictionary<string, string[]>? Errors);
    private sealed record ProblemPayload(string? Title, string? Detail);
#pragma warning restore CA1812

    [Fact]
    public async Task Delete_DeleteUser_ReturnsNoContentThenNotFoundOnGet_WhenUserExistsAndWasCached()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var newUser = new CreateRequest("Marlyn", "Wescoff", "marlyn.wescoff.delete@example.com");
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        created.ShouldNotBeNull();
        var userUri = new Uri(client.BaseAddress!, $"/api/v1/users/{created.Id}");
        var expectedBeforeDelete = new GetByIdResponse(created.Id, "Marlyn", "Wescoff", "marlyn.wescoff.delete@example.com");

        // Act
        var getBeforeResponse = await client.GetAsync(userUri, cancellationToken);
        var actualBeforeDelete = await getBeforeResponse.Content.ReadFromJsonAsync<GetByIdResponse>(cancellationToken);
        var deleteResponse = await client.DeleteAsync(userUri, cancellationToken);
        var getAfterResponse = await client.GetAsync(userUri, cancellationToken);

        // Assert
        getBeforeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        actualBeforeDelete.ShouldBeEquivalentTo(expectedBeforeDelete);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        getAfterResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DeleteUser_ReturnsOnlyNoContentOrNotFound_WhenDeletesRaceForTheSameUser()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        const int attempts = 6;
        var statuses = new List<HttpStatusCode>();

        // Act
        for (var round = 0; round < 10; round++)
        {
            var newUser = new CreateRequest("Mary", "Jackson", $"mary.jackson.race{round}@example.com");
            var createResponse = await client.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
            var created = await createResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
            created.ShouldNotBeNull();
            var userUri = new Uri(client.BaseAddress!, $"/api/v1/users/{created.Id}");

            var responses = await Task.WhenAll(Enumerable.Range(0, attempts).Select(_ => client.DeleteAsync(userUri, cancellationToken)));
            statuses.AddRange(responses.Select(response => response.StatusCode));
        }

        // Assert
        statuses.ShouldAllBe(status => status == HttpStatusCode.NoContent || status == HttpStatusCode.NotFound);
        statuses.Count(status => status == HttpStatusCode.NoContent).ShouldBe(10);
    }

    [Fact]
    public async Task Delete_DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var missingId = Guid.Parse("00000000-0000-0000-0000-0000000000ee");

        // Act
        var response = await client.DeleteAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{missingId}"), cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Entity 'User' was not found.");
        problem.Detail.ShouldBe("ERR-001");
    }

    [Fact]
    public async Task Delete_DeleteUser_ReturnsBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var response = await client.DeleteAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{Guid.Empty}"), cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrorsPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.ShouldNotBeNull();
        problem.Errors.ShouldNotBeNull();
        problem.Errors["Id"].ShouldBe(["Id is required and must be a valid guid format."]);
    }
}

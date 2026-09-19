using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Business.Users.GetById;
using Pocs.Aspire.Business.Users.Update;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Tests.Functional.Users;

[Collection(SharedAspireHost.Name)]
public class UserUpdateFunctionalTests
{
    private readonly AspireHostFixture _fixture;

    public UserUpdateFunctionalTests(AspireHostFixture fixture)
    {
        _fixture = fixture;
    }

    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Put_UpdateUser_ReturnsOk_WhenInputIsValid()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var newUser = new CreateRequest("Charles", "Babbage", "charles.babbage.update@example.com");
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        created.ShouldNotBeNull();

        var updateRequest = new UpdateRequest(created.Id, "Augusta", "King", "augusta.king.update@example.com");
        var expectedUpdate = new UpdateResponse(created.Id, "Augusta", "King", "augusta.king.update@example.com");
        var expectedGet = new GetByIdResponse(created.Id, "Augusta", "King", "augusta.king.update@example.com");

        // Act
        var updateResponse = await client.PutAsJsonAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{created.Id}"), updateRequest, cancellationToken);
        var actualUpdate = await updateResponse.Content.ReadFromJsonAsync<UpdateResponse>(cancellationToken);

        var getResponse = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{created.Id}"), cancellationToken);
        var actualGet = await getResponse.Content.ReadFromJsonAsync<GetByIdResponse>(cancellationToken);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        actualUpdate.ShouldBeEquivalentTo(expectedUpdate);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        actualGet.ShouldBeEquivalentTo(expectedGet);
    }

    [Fact]
    public async Task Put_UpdateUser_ReturnsBadRequest_WhenInputIsInvalid()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var id = Guid.Parse("00000000-0000-0000-0000-0000000000cc");
        var invalidUpdate = new UpdateRequest(id, "", "", "not-an-email");

        // Act
        var response = await client.PutAsJsonAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{id}"), invalidUpdate, cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrorsPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.ShouldNotBeNull();
        problem.Errors.ShouldNotBeNull();
        problem.Errors["FirstName"].ShouldBe(["First name is required."]);
        problem.Errors["LastName"].ShouldBe(["Last name is required."]);
        problem.Errors["Email"].ShouldBe(["A valid email is required."]);
    }

#pragma warning disable CA1812 // instantiated via JSON deserialization
    private sealed record ValidationErrorsPayload(string? Title, Dictionary<string, string[]>? Errors);
    private sealed record ProblemPayload(string? Title, string? Detail);
#pragma warning restore CA1812

    [Fact]
    public async Task Put_UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var missingId = Guid.Parse("00000000-0000-0000-0000-0000000000dd");
        var updateRequest = new UpdateRequest(missingId, "Missing", "User", "missing.user.update@example.com");

        // Act
        var response = await client.PutAsJsonAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{missingId}"), updateRequest, cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Entity 'User' was not found.");
        problem.Detail.ShouldBe("ERR-001");
    }

    [Fact]
    public async Task Put_UpdateUser_ReturnsConflict_WhenEmailBelongsToAnotherUser()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var userA = new CreateRequest("Grace", "Murray", "grace.murray.updateconflict@example.com");
        var userB = new CreateRequest("Herman", "Hollerith", "herman.hollerith.updateconflict@example.com");

        var createdAResponse = await client.PostAsJsonAsync("/api/v1/users", userA, cancellationToken);
        var createdA = await createdAResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        createdA.ShouldNotBeNull();

        var createdBResponse = await client.PostAsJsonAsync("/api/v1/users", userB, cancellationToken);
        var createdB = await createdBResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        createdB.ShouldNotBeNull();

        var updateRequest = new UpdateRequest(createdB.Id, "Herman", "Hollerith", "grace.murray.updateconflict@example.com");

        // Act
        var response = await client.PutAsJsonAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{createdB.Id}"), updateRequest, cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("The email grace.murray.updateconflict@example.com already exists.");
        problem.Detail.ShouldBe("ERR-003");
    }

    [Fact]
    public async Task Put_UpdateUser_ReturnsOk_WhenEmailIsUnchanged()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var newUser = new CreateRequest("Radia", "Perlman", "radia.perlman.sameemail@example.com");
        var createResponse = await client.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        created.ShouldNotBeNull();

        var updateRequest = new UpdateRequest(created.Id, "Radia Rose", "Perlman", "radia.perlman.sameemail@example.com");
        var expected = new UpdateResponse(created.Id, "Radia Rose", "Perlman", "radia.perlman.sameemail@example.com");

        // Act
        var response = await client.PutAsJsonAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{created.Id}"), updateRequest, cancellationToken);
        var actual = await response.Content.ReadFromJsonAsync<UpdateResponse>(cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        actual.ShouldBeEquivalentTo(expected);
    }
}

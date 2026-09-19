using Microsoft.AspNetCore.Http;
using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Business.Users.GetById;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Tests.Functional.Users;

[Collection(SharedAspireHost.Name)]
public class UserFunctionalTests
{
    private readonly AspireHostFixture _fixture;

    public UserFunctionalTests(AspireHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Post_CreateUser_ReturnsCreatedWithCorrectLocation_WhenInputIsValid()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var newUser = new CreateRequest("Test", "User", "test.user@example.com");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
        var actual = await response.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        actual.ShouldNotBeNull();
        actual.Id.ShouldNotBe(Guid.Empty);
        var expectedUri = new Uri(client.BaseAddress!, $"/api/v1/users/{actual.Id}");
        response.Headers.Location.ShouldBe(expectedUri);
    }

    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Post_CreateUser_ReturnsConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var newUser = new CreateRequest("Margaret", "Hamilton", "margaret.hamilton.conflict@example.com");
        var firstResponse = await client.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
        firstResponse.EnsureSuccessStatusCode();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("The email margaret.hamilton.conflict@example.com already exists.");
        problem.Detail.ShouldBe("ERR-003");
    }

    [Fact]
    public async Task Post_CreateUser_ReturnsBadRequestWithFieldErrors_WhenInputIsInvalid()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var invalidUser = new CreateRequest("", "", "not-an-email");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users", invalidUser, cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrorsPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.ShouldNotBeNull();
        problem.Errors.ShouldNotBeNull();
        problem.Errors["FirstName"].ShouldBe(["First name is required."]);
        problem.Errors["LastName"].ShouldBe(["Last name is required."]);
        problem.Errors["Email"].ShouldBe(["A valid email is required."]);
    }

    [Fact]
    public async Task Post_CreateUser_ReturnsBadRequest_WhenEmailExceedsMaxLength()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var overlongLocalPart = new string('a', 92);
        var overlongEmail = $"{overlongLocalPart}@example.com"; // 92 + 12 = 104 chars, valid format, over the 100-char DB column limit
        var invalidUser = new CreateRequest("Ada", "Lovelace", overlongEmail);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users", invalidUser, cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrorsPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.ShouldNotBeNull();
        problem.Errors.ShouldNotBeNull();
        problem.Errors["Email"].ShouldBe(["Emails cannot exceed 100 characters."]);
    }

#pragma warning disable CA1812 // instantiated via JSON deserialization
    private sealed record ValidationErrorsPayload(string? Title, Dictionary<string, string[]>? Errors);
    private sealed record ProblemPayload(string? Title, string? Detail);
#pragma warning restore CA1812

    [Fact]
    public async Task Get_GetById_ReturnsOkWithUser_WhenUserExists()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var newUser = new CreateRequest("Grace", "Hopper", "grace.hopper.getbyid@example.com");

        var createResponse = await client.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        created.ShouldNotBeNull();

        var expected = new GetByIdResponse(created.Id, "Grace", "Hopper", "grace.hopper.getbyid@example.com");

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{created.Id}"), cancellationToken);
        var actual = await response.Content.ReadFromJsonAsync<GetByIdResponse>(cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task Get_GetById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var missingId = Guid.Parse("00000000-0000-0000-0000-0000000000aa");

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{missingId}"), cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_GetById_ReturnsBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{Guid.Empty}"), cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrorsPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.ShouldNotBeNull();
        problem.Errors.ShouldNotBeNull();
        problem.Errors["Id"].ShouldBe(["Id is required and must be a valid guid format."]);
    }

    [Fact]
    public async Task Get_GetById_ReturnsEachUsersOwnData_WhenRequestedBackToBackWithinTheCacheWindow()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var userA = new CreateRequest("Alice", "Anderson", "alice.anderson.cache@example.com");
        var userB = new CreateRequest("Bob", "Baker", "bob.baker.cache@example.com");

        var createdAResponse = await client.PostAsJsonAsync("/api/v1/users", userA, cancellationToken);
        var createdA = await createdAResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        createdA.ShouldNotBeNull();

        var createdBResponse = await client.PostAsJsonAsync("/api/v1/users", userB, cancellationToken);
        var createdB = await createdBResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        createdB.ShouldNotBeNull();

        var expectedA = new GetByIdResponse(createdA.Id, "Alice", "Anderson", "alice.anderson.cache@example.com");
        var expectedB = new GetByIdResponse(createdB.Id, "Bob", "Baker", "bob.baker.cache@example.com");

        // Act
        var responseA = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{createdA.Id}"), cancellationToken);
        var actualA = await responseA.Content.ReadFromJsonAsync<GetByIdResponse>(cancellationToken);

        var responseB = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/v1/users/{createdB.Id}"), cancellationToken);
        var actualB = await responseB.Content.ReadFromJsonAsync<GetByIdResponse>(cancellationToken);

        // Assert
        actualA.ShouldBeEquivalentTo(expectedA);
        actualB.ShouldBeEquivalentTo(expectedB);
    }
}

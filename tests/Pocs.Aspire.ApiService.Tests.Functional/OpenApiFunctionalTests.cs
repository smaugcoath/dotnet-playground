using Shouldly;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Tests.Functional;

[Collection(SharedAspireHost.Name)]
public class OpenApiFunctionalTests
{
    private readonly AspireHostFixture _fixture;

    public OpenApiFunctionalTests(AspireHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Get_OpenApiDocument_ReturnsPathsForEveryUsersRoute()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, "/openapi/v1.json"), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(body);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pathKeys = document.RootElement.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToList();
        pathKeys.ShouldBe(["/api/v1/users", "/api/v1/users/{id}"], ignoreOrder: true);
        document.RootElement.GetProperty("info").GetProperty("title").GetString().ShouldBe("Pocs.Aspire.ApiService | v1");
    }

    [Fact]
    public async Task Get_OpenApiDocument_DescribesEveryUsersOperationWithItsValidationFailure()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var expected = new[]
        {
            "POST /api/v1/users: Creates a user. [400]",
            "GET /api/v1/users: Lists users, paged. [400]",
            "GET /api/v1/users/{id}: Retrieves a user by ID. [400]",
            "PUT /api/v1/users/{id}: Updates a user. [400]",
            "DELETE /api/v1/users/{id}: Deletes a user. [400]",
        };

        // Act
        var body = await client.GetStringAsync(new Uri(client.BaseAddress!, "/openapi/v1.json"), cancellationToken);
        using var document = JsonDocument.Parse(body);
        var actual = document.RootElement.GetProperty("paths").EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject().Select(operation => (Path: path.Name, Method: operation.Name.ToUpperInvariant(), Operation: operation.Value)))
            .Select(entry => $"{entry.Method} {entry.Path}: {entry.Operation.GetProperty("summary").GetString()} [{(entry.Operation.GetProperty("responses").TryGetProperty("400", out _) ? "400" : "no 400")}]")
            .ToList();

        // Assert
        actual.ShouldBe(expected, ignoreOrder: true);
    }
}

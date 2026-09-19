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
}

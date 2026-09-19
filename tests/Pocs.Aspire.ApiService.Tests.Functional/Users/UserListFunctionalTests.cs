using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Business.Users.List;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Tests.Functional.Users;

[Collection(SharedAspireHost.Name)]
public class UserListFunctionalTests
{
    private readonly AspireHostFixture _fixture;

    public UserListFunctionalTests(AspireHostFixture fixture)
    {
        _fixture = fixture;
    }

    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

#pragma warning disable CA1812 // instantiated via JSON deserialization
    private sealed record ValidationErrorsPayload(string? Title, Dictionary<string, string[]>? Errors);
#pragma warning restore CA1812

    [Fact]
    public async Task Get_ListUsers_ReturnsUsersInEmailOrder_AndPagesSliceThatOrder()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        var userA = new CreateRequest("Elsie", "Shutt", "elsie.shutt.list@example.com");
        var userB = new CreateRequest("Kay", "McNulty", "kay.mcnulty.list@example.com");
        var userC = new CreateRequest("Ruth", "Teitelbaum", "ruth.teitelbaum.list@example.com");

        var createdAResponse = await client.PostAsJsonAsync("/api/v1/users", userA, cancellationToken);
        var createdA = await createdAResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        createdA.ShouldNotBeNull();

        var createdBResponse = await client.PostAsJsonAsync("/api/v1/users", userB, cancellationToken);
        var createdB = await createdBResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        createdB.ShouldNotBeNull();

        var createdCResponse = await client.PostAsJsonAsync("/api/v1/users", userC, cancellationToken);
        var createdC = await createdCResponse.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken);
        createdC.ShouldNotBeNull();

        var expectedItemA = new ListItem(createdA.Id, "Elsie", "Shutt", "elsie.shutt.list@example.com");
        var expectedItemB = new ListItem(createdB.Id, "Kay", "McNulty", "kay.mcnulty.list@example.com");
        var expectedItemC = new ListItem(createdC.Id, "Ruth", "Teitelbaum", "ruth.teitelbaum.list@example.com");

        // Act
        var fullResponse = await client.GetAsync(new Uri(client.BaseAddress!, "/api/v1/users?page=1&pageSize=100"), cancellationToken);
        var full = await fullResponse.Content.ReadFromJsonAsync<ListResponse>(cancellationToken);

        var page1Response = await client.GetAsync(new Uri(client.BaseAddress!, "/api/v1/users?page=1&pageSize=2"), cancellationToken);
        var page1 = await page1Response.Content.ReadFromJsonAsync<ListResponse>(cancellationToken);

        var page2Response = await client.GetAsync(new Uri(client.BaseAddress!, "/api/v1/users?page=2&pageSize=2"), cancellationToken);
        var page2 = await page2Response.Content.ReadFromJsonAsync<ListResponse>(cancellationToken);

        // Assert
        fullResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        full.ShouldNotBeNull();
        full.Page.ShouldBe(1);
        full.PageSize.ShouldBe(100);
        full.TotalCount.ShouldBe(full.Items.Count);
        full.Items.Select(item => item.Email).ShouldBe(full.Items.Select(item => item.Email).Order(StringComparer.Ordinal));
        full.Items.Where(item => item.Email.EndsWith(".list@example.com", StringComparison.Ordinal))
            .ShouldBe([expectedItemA, expectedItemB, expectedItemC], ignoreOrder: false);

        page1Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        page2Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        page1.ShouldNotBeNull();
        page2.ShouldNotBeNull();
        page1.ShouldBeEquivalentTo(new ListResponse(full.Items.Take(2).ToList(), 1, 2, full.TotalCount));
        page2.ShouldBeEquivalentTo(new ListResponse(full.Items.Skip(2).Take(2).ToList(), 2, 2, full.TotalCount));
    }

    [Fact]
    public async Task Get_ListUsers_ReturnsBadRequest_WhenPageSizeIsZero()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, "/api/v1/users?page=1&pageSize=0"), cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrorsPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.ShouldNotBeNull();
        problem.Errors.ShouldNotBeNull();
        problem.Errors["PageSize"].ShouldBe(["PageSize must be between 1 and 100."]);
    }

    [Fact]
    public async Task Get_ListUsers_ReturnsBadRequest_WhenPageIsZero()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, "/api/v1/users?page=0&pageSize=20"), cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrorsPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.ShouldNotBeNull();
        problem.Errors.ShouldNotBeNull();
        problem.Errors["Page"].ShouldBe(["Page must be 1 or greater."]);
    }

    [Fact]
    public async Task Get_ListUsers_ReturnsBadRequest_WhenPageSizeExceedsMaximum()
    {
        // Arrange
        var client = _fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, "/api/v1/users?page=1&pageSize=101"), cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrorsPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.ShouldNotBeNull();
        problem.Errors.ShouldNotBeNull();
        problem.Errors["PageSize"].ShouldBe(["PageSize must be between 1 and 100."]);
    }
}

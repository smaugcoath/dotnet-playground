using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Pocs.Aspire.ApiService.Extensions;
using Pocs.Aspire.Domain.Errors;
using Shouldly;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Tests.Functional.Extensions;

/// <summary>
/// Proves the failure-to-response mapping over a real HTTP round trip for the failures
/// that no endpoint can produce through normal client input.
/// </summary>
public class ProblemDetailsFunctionalTests : IAsyncLifetime
{
    private WebApplication _app = default!;
    private HttpClient _client = default!;

    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

#pragma warning disable CA1812 // instantiated via JSON deserialization
    private sealed record ProblemPayload(string? Title, string? Detail, int? Status);
#pragma warning restore CA1812

    private sealed record UnmappedFailure() : Failure("ERR-TEST", "A failure with no dedicated mapping.");

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        _app = builder.Build();
        _app.MapGet("/unique-constraint", () => new UniqueConstraintViolationError("IX_User_Other").ToProblem());
        _app.MapGet("/unmapped", () => new UnmappedFailure().ToProblem());
        await _app.StartAsync(TestContext.Current.CancellationToken);
        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.First()) };
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task ToProblem_ReturnsConflictWithConstraintCode_WhenAnyUniqueConstraintIsViolated()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var expected = new ProblemPayload("A conflicting update occurred.", "ERR-004", 409);

        // Act
        var response = await _client.GetAsync(new Uri("/unique-constraint", UriKind.Relative), cancellationToken);
        var actual = await response.Content.ReadFromJsonAsync<ProblemPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task ToProblem_ReturnsInternalServerError_WhenTheFailureHasNoDedicatedMapping()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var expected = new ProblemPayload("Unexpected failure", "ERR-TEST", 500);

        // Act
        var response = await _client.GetAsync(new Uri("/unmapped", UriKind.Relative), cancellationToken);
        var actual = await response.Content.ReadFromJsonAsync<ProblemPayload>(CaseInsensitiveJson, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        actual.ShouldBeEquivalentTo(expected);
    }
}

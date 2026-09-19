using Microsoft.EntityFrameworkCore;
using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Infrastructure.Persistence;
using Shouldly;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.ApiService.Tests.Functional.Persistence;

[Collection(SharedAspireHost.Name)]
public class DatabaseMigrationFunctionalTests
{
    private readonly AspireHostFixture _fixture;

    public DatabaseMigrationFunctionalTests(AspireHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Startup_AppliesInitialMigration_ToPostgres()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        await EnsureApiServiceHasStartedUpAgainstPostgresAsync(cancellationToken);

        var connectionString = await _fixture.App.GetConnectionStringAsync("postgresdb", cancellationToken);
        connectionString.ShouldNotBeNull();

        await using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options);

        // Act
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);

        // Assert
        appliedMigrations.ShouldContain("20250411144525_InitialMigration");
    }

    private async Task EnsureApiServiceHasStartedUpAgainstPostgresAsync(CancellationToken cancellationToken)
    {
        var newUser = new CreateRequest("Migration", "Check", "migration.check@example.com");
        var warmupResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/v1/users", newUser, cancellationToken);
        warmupResponse.EnsureSuccessStatusCode();
    }
}

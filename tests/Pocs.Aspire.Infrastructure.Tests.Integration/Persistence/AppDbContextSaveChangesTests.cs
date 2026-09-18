namespace Pocs.Aspire.Infrastructure.Tests.Integration.Persistence;

using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;
using Pocs.Aspire.Infrastructure.Persistence;
using Shouldly;

using System;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

public class AppDbContextSaveChangesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private DbContextOptions<AppDbContext> DbContextOptions =>
    new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(_container.GetConnectionString())
        .Options;

    public AppDbContextSaveChangesTests()
    {
        var postgreSqlBuilder = new PostgreSqlBuilder("postgres:15");
        _container = postgreSqlBuilder
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);
        using var context = new AppDbContext(DbContextOptions);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsUniqueConstraintViolationError_WhenTwoUsersShareAnEmail()
    {
        // Arrange
        var sharedEmail = Email.From("ada.lovelace.saveconflict@example.com");
        var seededUser = User.From(
            UserId.From(Guid.Parse("22222222-2222-2222-2222-222222222222")),
            FirstName.From("Ada"),
            LastName.From("Lovelace"),
            sharedEmail);

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.Add(seededUser);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var conflictingUser = User.From(
            UserId.From(Guid.Parse("33333333-3333-3333-3333-333333333333")),
            FirstName.From("Grace"),
            LastName.From("Hopper"),
            sharedEmail);

        await using var context = new AppDbContext(DbContextOptions);
        context.Add(conflictingUser);
        IUnitOfWork unitOfWork = context;
        var expected = Either<Failure, Unit>.Left(new UniqueConstraintViolationError(User.EmailUniqueIndexName));

        // Act
        var actual = await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsUnitDefault_WhenSaveSucceeds()
    {
        // Arrange
        var user = User.From(
            UserId.From(Guid.Parse("44444444-4444-4444-4444-444444444444")),
            FirstName.From("Grace"),
            LastName.From("Hopper"),
            Email.From("grace.hopper.saveok@example.com"));

        await using var context = new AppDbContext(DbContextOptions);
        context.Add(user);
        IUnitOfWork unitOfWork = context;
        var expected = Either<Failure, Unit>.Right(Unit.Default);

        // Act
        var actual = await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }
}

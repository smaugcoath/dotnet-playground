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
        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsEmailAlreadyExistsError_WhenAnInsertedUserReusesAnEmail()
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
        var expected = Either<Failure, Unit>.Left(new EmailAlreadyExistsError(sharedEmail));

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

    [Fact]
    public async Task SaveChangesAsync_ReturnsNotFoundError_WhenTheDeletedUserWasAlreadyRemoved()
    {
        // Arrange
        var user = User.From(
            UserId.From(Guid.Parse("55555555-5555-5555-5555-555555555555")),
            FirstName.From("Katherine"),
            LastName.From("Johnson"),
            Email.From("katherine.johnson.concurrency@example.com"));

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.Add(user);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var firstContext = new AppDbContext(DbContextOptions);
        await using var secondContext = new AppDbContext(DbContextOptions);
        var firstCopy = await firstContext.Users.SingleAsync(TestContext.Current.CancellationToken);
        var secondCopy = await secondContext.Users.SingleAsync(TestContext.Current.CancellationToken);
        firstContext.Remove(firstCopy);
        await ((IUnitOfWork)firstContext).SaveChangesAsync(TestContext.Current.CancellationToken);
        secondContext.Remove(secondCopy);
        var expected = Either<Failure, Unit>.Left(new NotFoundError(nameof(User)));

        // Act
        var actual = await ((IUnitOfWork)secondContext).SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsEmailAlreadyExistsError_WhenAnUpdatedUserTakesAnotherUsersEmail()
    {
        // Arrange
        var takenEmail = Email.From("taken.update.race@example.com");
        var owner = User.From(
            UserId.From(Guid.Parse("66666666-6666-6666-6666-666666666666")),
            FirstName.From("Ada"),
            LastName.From("Lovelace"),
            takenEmail);
        var updater = User.From(
            UserId.From(Guid.Parse("77777777-7777-7777-7777-777777777777")),
            FirstName.From("Grace"),
            LastName.From("Hopper"),
            Email.From("grace.update.race@example.com"));

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.AddRange(owner, updater);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var context = new AppDbContext(DbContextOptions);
        var tracked = await context.Users.SingleAsync(u => u.Id == updater.Id, TestContext.Current.CancellationToken);
        tracked.Email = takenEmail;
        var expected = Either<Failure, Unit>.Left(new EmailAlreadyExistsError(takenEmail));

        // Act
        var actual = await ((IUnitOfWork)context).SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsNotFoundError_WhenTheUpdatedUserWasAlreadyRemoved()
    {
        // Arrange
        var user = User.From(
            UserId.From(Guid.Parse("88888888-8888-8888-8888-888888888888")),
            FirstName.From("Mary"),
            LastName.From("Jackson"),
            Email.From("mary.jackson.concurrency@example.com"));

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.Add(user);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var deleteContext = new AppDbContext(DbContextOptions);
        await using var updateContext = new AppDbContext(DbContextOptions);
        var toDelete = await deleteContext.Users.SingleAsync(TestContext.Current.CancellationToken);
        var toUpdate = await updateContext.Users.SingleAsync(TestContext.Current.CancellationToken);
        deleteContext.Remove(toDelete);
        await ((IUnitOfWork)deleteContext).SaveChangesAsync(TestContext.Current.CancellationToken);
        toUpdate.FirstName = FirstName.From("Maria");
        var expected = Either<Failure, Unit>.Left(new NotFoundError(nameof(User)));

        // Act
        var actual = await ((IUnitOfWork)updateContext).SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }
}

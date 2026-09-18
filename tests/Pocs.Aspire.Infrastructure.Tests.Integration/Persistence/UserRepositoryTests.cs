namespace Pocs.Aspire.Infrastructure.Tests.Integration.Persistence;

using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;
using Pocs.Aspire.Infrastructure.Persistence;
using Shouldly;

using System;
using System.Threading;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private DbContextOptions<AppDbContext> DbContextOptions =>
    new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(_container.GetConnectionString())
        .Options;

    public UserRepositoryTests()
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
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var expected = User.From(
            UserId.New(),
            FirstName.From("Ada"),
            LastName.From("Lovelace"),
            Email.From("ada.lovelace.getbyid@example.com"));

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.Add(expected);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var context = new AppDbContext(DbContextOptions);
        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetByIdAsync(expected.Id, TestContext.Current.CancellationToken);

        // Assert
        var actual = result.Match(
            Some: user => user,
            None: () => throw new InvalidOperationException("Expected the user to be found, but the repository returned None."));

        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsOperationCanceled_WhenCancellationTokenIsAlreadyCanceled()
    {
        // Arrange
        await using var context = new AppDbContext(DbContextOptions);
        var repository = new UserRepository(context);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => repository.GetByIdAsync(UserId.New(), cts.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNone_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context = new AppDbContext(DbContextOptions);
        var repository = new UserRepository(context);
        var missingId = UserId.New();
        var expected = Option<User>.None;

        // Act
        var result = await repository.GetByIdAsync(missingId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task CreateAsync_PersistsUser()
    {
        // Arrange
        var expected = User.From(
            UserId.New(),
            FirstName.From("Katherine"),
            LastName.From("Johnson"),
            Email.From("katherine.johnson.create@example.com"));

        await using (var context = new AppDbContext(DbContextOptions))
        {
            var repository = new UserRepository(context);
            await repository.CreateAsync(expected, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        await using var readContext = new AppDbContext(DbContextOptions);
        var readRepository = new UserRepository(readContext);
        var result = await readRepository.GetByIdAsync(expected.Id, TestContext.Current.CancellationToken);

        // Assert
        var actual = result.Match(
            Some: user => user,
            None: () => throw new InvalidOperationException("Expected the user to be found, but the repository returned None."));

        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        // Arrange
        var seeded = User.From(
            UserId.New(),
            FirstName.From("Hedy"),
            LastName.From("Lamarr"),
            Email.From("hedy.lamarr.update@example.com"));

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.Add(seeded);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var expected = User.From(
            seeded.Id,
            FirstName.From("Hedwig"),
            LastName.From("Kiesler"),
            Email.From("hedwig.kiesler.update@example.com"));

        await using (var updateContext = new AppDbContext(DbContextOptions))
        {
            var repository = new UserRepository(updateContext);
            var loaded = (await repository.GetByIdAsync(seeded.Id, TestContext.Current.CancellationToken)).Match(
                Some: user => user,
                None: () => throw new InvalidOperationException("Expected the seeded user to be found, but the repository returned None."));

            (loaded.FirstName, loaded.LastName, loaded.Email) = (FirstName.From("Hedwig"), LastName.From("Kiesler"), Email.From("hedwig.kiesler.update@example.com"));

            await repository.UpdateAsync(loaded, TestContext.Current.CancellationToken);
            await updateContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        await using var readContext = new AppDbContext(DbContextOptions);
        var readRepository = new UserRepository(readContext);
        var result = await readRepository.GetByIdAsync(seeded.Id, TestContext.Current.CancellationToken);

        // Assert
        var actual = result.Match(
            Some: user => user,
            None: () => throw new InvalidOperationException("Expected the user to be found, but the repository returned None."));

        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task EmailExistsExceptForUser_ReturnsTrue_WhenAnotherUserHasTheEmail()
    {
        // Arrange
        var userA = User.From(
            UserId.New(),
            FirstName.From("Annie"),
            LastName.From("Easley"),
            Email.From("annie.easley.emailexists@example.com"));
        var userB = User.From(
            UserId.New(),
            FirstName.From("Mary"),
            LastName.From("Jackson"),
            Email.From("mary.jackson.emailexists@example.com"));

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.AddRange(userA, userB);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var context = new AppDbContext(DbContextOptions);
        var repository = new UserRepository(context);

        // Act
        var result = await repository.EmailExistsExceptForUser(userA.Email, userB.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task EmailExistsExceptForUser_ReturnsFalse_WhenOnlyTheSameUserHasTheEmail()
    {
        // Arrange
        var user = User.From(
            UserId.New(),
            FirstName.From("Dorothy"),
            LastName.From("Vaughan"),
            Email.From("dorothy.vaughan.emailexists@example.com"));

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.Add(user);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var context = new AppDbContext(DbContextOptions);
        var repository = new UserRepository(context);

        // Act
        var result = await repository.EmailExistsExceptForUser(user.Email, user.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task EmailExistsExceptForUser_ReturnsFalse_WhenNoUserHasTheEmail()
    {
        // Arrange
        var other = User.From(
            UserId.New(),
            FirstName.From("Melba"),
            LastName.From("Roy"),
            Email.From("melba.roy.emailexists@example.com"));

        await using (var seedContext = new AppDbContext(DbContextOptions))
        {
            seedContext.Add(other);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var context = new AppDbContext(DbContextOptions);
        var repository = new UserRepository(context);
        var email = Email.From("no.one.emailexists@example.com");

        // Act
        var result = await repository.EmailExistsExceptForUser(email, other.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }
}

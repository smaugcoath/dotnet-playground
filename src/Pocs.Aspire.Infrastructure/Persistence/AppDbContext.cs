using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Infrastructure.Persistence.Configurations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    internal DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    async Task<Either<Failure, Unit>> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.SaveChangesAsync(cancellationToken);

            return Unit.Default;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return ToNotFound(ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pgEx)
        {
            return ToUniquenessFailure(ex, pgEx.ConstraintName);
        }
    }

    private static NotFoundError ToNotFound(DbUpdateConcurrencyException ex) =>
        new(ex.Entries.Count > 0 ? ex.Entries[0].Metadata.ClrType.Name : nameof(User));

    private static Failure ToUniquenessFailure(DbUpdateException ex, string? constraintName) =>
        constraintName == UserConfiguration.EmailUniqueIndexName
            && ex.Entries.Select(entry => entry.Entity).OfType<User>().FirstOrDefault() is { } user
            ? new EmailAlreadyExistsError(user.Email)
            : new UniqueConstraintViolationError();
}

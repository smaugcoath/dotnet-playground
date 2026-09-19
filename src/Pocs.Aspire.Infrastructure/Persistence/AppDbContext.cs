using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
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
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pgEx)
        {
            return new UniqueConstraintViolationError(pgEx.ConstraintName ?? string.Empty);
        }
    }
}

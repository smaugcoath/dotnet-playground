using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Infrastructure.Persistence;

namespace Pocs.Aspire.Infrastructure;

public static class HostApplicationBuilderCollectionExtensions
{
    public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<AppDbContext>("postgresdb");
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        return builder;
    }

    public static void ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
}

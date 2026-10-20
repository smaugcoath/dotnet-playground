using System.Threading.Tasks;

using Aspire.Hosting;

namespace Pocs.Aspire.AppHost;
internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var cache = builder.AddRedis("cache")
            .WithRedisInsight();

        var postgres = builder
            .AddPostgres("postgres")
            .WithImage("postgres")
            .WithImageTag("15")
            .WithPgAdmin();


        var postgresDb = postgres
            //.WithDataVolume(isReadOnly: false)
            .AddDatabase("postgresdb");

        builder.AddProject<Projects.Pocs_Aspire_ApiService>("apiservice")
            .WithReference(postgresDb)
            .WaitFor(postgresDb)
            .WithReference(cache)
            .WaitFor(cache);


        var app = builder.Build();

        await app.RunAsync();
    }


}

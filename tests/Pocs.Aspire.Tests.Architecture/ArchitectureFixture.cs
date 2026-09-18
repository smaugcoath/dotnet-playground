namespace Pocs.Aspire.Tests.Architecture;

using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using SystemAssembly = System.Reflection.Assembly;

internal static class ArchitectureFixture
{
    // IncludingDependencies pulls in one level of each assembly's own referenced types (EF Core,
    // ASP.NET Core, Npgsql, ...) so the namespace-based rules have real types to match against;
    // plain LoadAssemblies leaves those namespaces empty and the rules pass vacuously.
    internal static readonly Architecture Architecture = new ArchLoader().LoadAssembliesIncludingDependencies(
        SystemAssembly.Load("Pocs.Aspire.Domain"),
        SystemAssembly.Load("Pocs.Aspire.Business"),
        SystemAssembly.Load("Pocs.Aspire.Infrastructure"),
        SystemAssembly.Load("Pocs.Aspire.ApiService"),
        SystemAssembly.Load("Pocs.Aspire.ServiceDefaults"),
        SystemAssembly.Load("Pocs.Aspire.AppHost")).Build();
}

namespace Pocs.Aspire.Tests.Architecture;

using ArchUnitNET.Fluent.Syntax.Elements.Types;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

// Dependencies between the project assemblies themselves (Domain -> Business, Business -> Infrastructure,
// Infrastructure -> ApiService, ...) are already impossible by the csproj ProjectReference layout, so only
// rules the compiler does not enforce live here. Each rule requires its subject types to match: a
// mistyped assembly regex fails the test instead of passing on an empty set.
public class LayerDependencyTests
{
    private const string CoverageInstrumentationNamespace = "^Microsoft\\.CodeCoverage";

    private static GivenTypesConjunction ProjectTypes(string assemblyPrefix) =>
        Types().That().ResideInAssemblyMatching(assemblyPrefix)
            .And().DoNotResideInNamespaceMatching(CoverageInstrumentationNamespace);

    [Fact]
    public void Domain_DoesNotDependOnEfCoreOrAspNetCore()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Domain,")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching("Microsoft\\.EntityFrameworkCore.*")
                .Or().ResideInNamespaceMatching("Microsoft\\.AspNetCore.*")
                .Or().ResideInNamespaceMatching("Npgsql.*"))
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Business_DoesNotDependOnEfCore()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Business,")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching("Microsoft\\.EntityFrameworkCore.*")
                .Or().ResideInNamespaceMatching("Npgsql.*"))
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnBusiness()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Infrastructure,")
            .Should().NotDependOnAny(ProjectTypes("^Pocs\\.Aspire\\.Business,"))
            .Check(ArchitectureFixture.Architecture);
    }
}

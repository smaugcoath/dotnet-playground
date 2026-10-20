namespace Pocs.Aspire.Tests.Architecture;

using ArchUnitNET.Fluent.Syntax.Elements.Types;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

public class LayerDependencyTests
{
    private const string CoverageInstrumentationNamespace = "^Microsoft\\.CodeCoverage";

    private static GivenTypesConjunction ProjectTypes(string assemblyPrefix) =>
        Types().That().ResideInAssemblyMatching(assemblyPrefix)
            .And().DoNotResideInNamespaceMatching(CoverageInstrumentationNamespace);

    [Fact]
    public void Domain_DoesNotDependOnAnyOtherProject()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Domain,")
            .Should().NotDependOnAny(ProjectTypes("^Pocs\\.Aspire\\.Business,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Infrastructure,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.ApiService,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.ServiceDefaults,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.AppHost,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Domain_DoesNotDependOnEfCoreOrAspNetCore()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Domain,")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching("Microsoft\\.EntityFrameworkCore.*")
                .Or().ResideInNamespaceMatching("Microsoft\\.AspNetCore.*")
                .Or().ResideInNamespaceMatching("Npgsql.*"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Business_DoesNotDependOnInfrastructureOrApiService()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Business,")
            .Should().NotDependOnAny(ProjectTypes("^Pocs\\.Aspire\\.Infrastructure,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.ApiService,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.AppHost,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Business_DoesNotDependOnEfCore()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Business,")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching("Microsoft\\.EntityFrameworkCore.*")
                .Or().ResideInNamespaceMatching("Npgsql.*"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApiServiceOrAppHost()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Infrastructure,")
            .Should().NotDependOnAny(ProjectTypes("^Pocs\\.Aspire\\.ApiService,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.AppHost,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnBusiness()
    {
        ProjectTypes("^Pocs\\.Aspire\\.Infrastructure,")
            .Should().NotDependOnAny(ProjectTypes("^Pocs\\.Aspire\\.Business,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void ServiceDefaults_DoesNotDependOnAnyOtherProject()
    {
        ProjectTypes("^Pocs\\.Aspire\\.ServiceDefaults,")
            .Should().NotDependOnAny(ProjectTypes("^Pocs\\.Aspire\\.Domain,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Business,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Infrastructure,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.ApiService,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.AppHost,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }
}

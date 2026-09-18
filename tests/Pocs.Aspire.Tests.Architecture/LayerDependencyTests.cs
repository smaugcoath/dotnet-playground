namespace Pocs.Aspire.Tests.Architecture;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_DoesNotDependOnAnyOtherProject()
    {
        Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Domain,")
            .Should().NotDependOnAny(Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Business,")
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
        Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Domain,")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching("Microsoft\\.EntityFrameworkCore.*")
                .Or().ResideInNamespaceMatching("Microsoft\\.AspNetCore.*")
                .Or().ResideInNamespaceMatching("Npgsql.*"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Business_DoesNotDependOnInfrastructureOrApiService()
    {
        Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Business,")
            .Should().NotDependOnAny(Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Infrastructure,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.ApiService,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.AppHost,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Business_DoesNotDependOnEfCore()
    {
        Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Business,")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching("Microsoft\\.EntityFrameworkCore.*")
                .Or().ResideInNamespaceMatching("Npgsql.*"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApiServiceOrAppHost()
    {
        Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Infrastructure,")
            .Should().NotDependOnAny(Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.ApiService,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.AppHost,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnBusiness()
    {
        Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Infrastructure,")
            .Should().NotDependOnAny(Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Business,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    [Fact]
    public void ServiceDefaults_DoesNotDependOnAnyOtherProject()
    {
        Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.ServiceDefaults,")
            .Should().NotDependOnAny(Types().That().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Domain,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Business,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.Infrastructure,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.ApiService,")
                .Or().ResideInAssemblyMatching("^Pocs\\.Aspire\\.AppHost,"))
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }
}

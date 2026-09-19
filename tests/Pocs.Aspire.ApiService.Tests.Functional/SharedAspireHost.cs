using Xunit;

namespace Pocs.Aspire.ApiService.Tests.Functional;

[CollectionDefinition(Name)]
public sealed class SharedAspireHost : ICollectionFixture<AspireHostFixture>
{
    public const string Name = "AspireHost";
}

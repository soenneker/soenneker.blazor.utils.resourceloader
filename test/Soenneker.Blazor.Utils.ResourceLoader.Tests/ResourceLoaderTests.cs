using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;


namespace Soenneker.Blazor.Utils.ResourceLoader.Tests;

[Collection("Collection")]
public class ResourceLoaderTests : FixturedUnitTest
{
    private readonly IResourceLoader _util;

    public ResourceLoaderTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IResourceLoader>(true);
    }

    [Fact]
    public void Default()
    {

    }
}

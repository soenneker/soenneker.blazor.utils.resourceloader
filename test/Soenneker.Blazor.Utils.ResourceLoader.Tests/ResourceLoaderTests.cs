using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blazor.Utils.ResourceLoader.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class ResourceLoaderTests : HostedUnitTest
{
    private readonly IResourceLoader _util;

    public ResourceLoaderTests(Host host) : base(host)
    {
        _util = Resolve<IResourceLoader>(true);
    }

    [Test]
    public void Default()
    {

    }
}

using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Blazor.Utils.ResourceLoader;

///<inheritdoc cref="IResourceLoader"/>
public class ResourceLoader : IResourceLoader
{
    private readonly IJSRuntime _jsRuntime;

    public ResourceLoader(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask LoadScript(string uri, string integrity, CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsync("ResourceLoader.loadScript", cancellationToken, uri, integrity);
    }

    public ValueTask LoadStyle(string uri, string integrity, CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsync("ResourceLoader.loadStyle", cancellationToken, uri, integrity);
    }
}
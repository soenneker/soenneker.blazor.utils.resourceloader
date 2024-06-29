using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Soenneker.Blazor.Utils.ResourceLoader;

///<inheritdoc cref="IResourceLoader"/>
public class ResourceLoader : IResourceLoader
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ConcurrentDictionary<string, Task> _loadingScripts = new();
    private readonly ConcurrentDictionary<string, Task> _loadingStyles = new();

    public ResourceLoader(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    public Task LoadScript(string uri, string integrity, CancellationToken cancellationToken = default)
    {
        return _loadingScripts.GetOrAdd(uri, _ => LoadScriptInternal(uri, integrity, cancellationToken));
    }

    public Task LoadStyle(string uri, string integrity, CancellationToken cancellationToken = default)
    {
        return _loadingStyles.GetOrAdd(uri, _ => LoadStyleInternal(uri, integrity, cancellationToken));
    }

    private Task LoadScriptInternal(string uri, string integrity, CancellationToken cancellationToken)
    {
        return _jsRuntime.InvokeVoidAsync("ResourceLoader.loadScript", cancellationToken, uri, integrity).AsTask();
    }

    private Task LoadStyleInternal(string uri, string integrity, CancellationToken cancellationToken)
    {
        return _jsRuntime.InvokeVoidAsync("ResourceLoader.loadStyle", cancellationToken, uri, integrity).AsTask();
    }
}
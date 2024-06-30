using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Utils.AsyncSingleton;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Blazor.Utils.JsVariable.Abstract;

namespace Soenneker.Blazor.Utils.ResourceLoader;

///<inheritdoc cref="IResourceLoader"/>
public class ResourceLoader : IResourceLoader
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IJsVariableInterop _jsVariableInterop;
    private readonly ConcurrentDictionary<string, Task> _loadingScripts = new();
    private readonly ConcurrentDictionary<string, Task> _loadingStyles = new();

    private readonly AsyncSingleton<object> _scriptInitializer;

    public ResourceLoader(IJSRuntime jsRuntime, IModuleImportUtil moduleImportUtil, IJsVariableInterop jsVariableInterop)
    {
        _jsRuntime = jsRuntime;
        _jsVariableInterop = jsVariableInterop;

        _scriptInitializer = new AsyncSingleton<object>(async objects => {

            var cancellationToken = (CancellationToken)objects[0];

            await moduleImportUtil.Import("Soenneker.Blazor.Utils.ResourceLoader/resourceloader.js", cancellationToken);
            await moduleImportUtil.WaitUntilLoadedAndAvailable("Soenneker.Blazor.Utils.ResourceLoader/resourceloader.js", "ResourceLoader", 100, cancellationToken);

            return new object();
        });
    }

    public async ValueTask LoadScript(string uri, string integrity, CancellationToken cancellationToken = default)
    {
        _ = await _scriptInitializer.Get(cancellationToken).NoSync();
        await _loadingScripts.GetOrAdd(uri, _ => LoadScriptInternal(uri, integrity, cancellationToken)).NoSync();
    }

    public async ValueTask LoadStyle(string uri, string integrity, CancellationToken cancellationToken = default)
    {
        _ = await _scriptInitializer.Get(cancellationToken).NoSync();
        await _loadingStyles.GetOrAdd(uri, _ => LoadStyleInternal(uri, integrity, cancellationToken)).NoSync();
    }

    private Task LoadScriptInternal(string uri, string integrity, CancellationToken cancellationToken)
    {
        return _jsRuntime.InvokeVoidAsync("ResourceLoader.loadScript", cancellationToken, uri, integrity).AsTask();
    }

    private Task LoadStyleInternal(string uri, string integrity, CancellationToken cancellationToken)
    {
        return _jsRuntime.InvokeVoidAsync("ResourceLoader.loadStyle", cancellationToken, uri, integrity).AsTask();
    }

    public ValueTask WaitForVariable(string variableName, int delay = 100, CancellationToken cancellationToken = default)
    {
        return _jsVariableInterop.WaitForVariable(variableName, delay, cancellationToken);
    }
}
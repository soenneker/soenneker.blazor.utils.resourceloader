using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using System.Threading.Tasks;
using System.Threading;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Utils.AsyncSingleton;
using Soenneker.Extensions.ValueTask;
using Soenneker.Blazor.Utils.JsVariable.Abstract;
using Soenneker.Utils.SingletonDictionary;

namespace Soenneker.Blazor.Utils.ResourceLoader;

///<inheritdoc cref="IResourceLoader"/>
public class ResourceLoader : IResourceLoader
{
    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly IJsVariableInterop _jsVariableInterop;
    private readonly SingletonDictionary<object> _scripts;
    private readonly SingletonDictionary<object> _styles;

    private readonly AsyncSingleton<object> _initializer;

    public ResourceLoader(IJSRuntime jsRuntime, IModuleImportUtil moduleImportUtil, IJsVariableInterop jsVariableInterop)
    {
        _moduleImportUtil = moduleImportUtil;
        _jsVariableInterop = jsVariableInterop;

        _initializer = new AsyncSingleton<object>(async objects =>
        {
            var cancellationToken = (CancellationToken) objects[0];

            await _moduleImportUtil.ImportAndWaitUntilAvailable("Soenneker.Blazor.Utils.ResourceLoader/resourceloader.js", "ResourceLoader", 100, cancellationToken);

            return new object();
        });

        _scripts = new SingletonDictionary<object>(async (uri, objects) =>
        {
            var integrity = (string?) objects[0];
            var cancellationToken = (CancellationToken) objects[1];

            await jsRuntime.InvokeVoidAsync("ResourceLoader.loadScript", cancellationToken, uri, integrity);

            return new object();
        });

        _styles = new SingletonDictionary<object>(async (uri, objects) =>
        {
            var integrity = (string?) objects[0];
            var cancellationToken = (CancellationToken) objects[1];

            await jsRuntime.InvokeVoidAsync("ResourceLoader.loadStyle", cancellationToken, uri, integrity);

            return new object();
        });
    }

    public async ValueTask LoadScript(string uri, string? integrity = null, CancellationToken cancellationToken = default)
    {
        _ = await _initializer.Get(cancellationToken).NoSync();
        _ = await _scripts.Get(uri, integrity!, cancellationToken).NoSync();
    }

    public async ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, CancellationToken cancellationToken = default)
    {
        await LoadScript(uri, integrity, cancellationToken).NoSync();
        await WaitForVariable(variableName, cancellationToken: cancellationToken).NoSync();
    }

    public async ValueTask LoadStyle(string uri, string? integrity, CancellationToken cancellationToken = default)
    {
        _ = await _initializer.Get(cancellationToken).NoSync();
        _ = await _styles.Get(uri, integrity!, cancellationToken).NoSync();
    }

    public ValueTask<IJSObjectReference> ImportModule(string name, CancellationToken cancellationToken = default)
    {
        return _moduleImportUtil.Import(name, cancellationToken);
    }

    public ValueTask ImportModuleAndWait(string name, CancellationToken cancellationToken = default)
    {
        return _moduleImportUtil.ImportAndWait(name, cancellationToken);
    }

    public ValueTask ImportModuleAndWaitUntilAvailable(string name, string variableName, int delay = 100, CancellationToken cancellationToken = default)
    {
        return _moduleImportUtil.ImportAndWaitUntilAvailable(name, variableName, delay, cancellationToken);
    }

    public ValueTask WaitForVariable(string variableName, int delay = 100, CancellationToken cancellationToken = default)
    {
        return _jsVariableInterop.WaitForVariable(variableName, delay, cancellationToken);
    }

    public ValueTask DisposeModule(string name, CancellationToken cancellationToken = default)
    {
        return _moduleImportUtil.DisposeModule(name, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _moduleImportUtil.DisposeModule("Soenneker.Blazor.Utils.ResourceLoader/resourceloader.js");
    }
}
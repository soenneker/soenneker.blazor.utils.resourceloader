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
public sealed class ResourceLoader : IResourceLoader
{
    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly IJsVariableInterop _jsVariableInterop;

    private readonly SingletonDictionary<object> _scripts;
    private readonly SingletonDictionary<object> _styles;

    private readonly AsyncSingleton _initializer;

    private const string _modulePath = "Soenneker.Blazor.Utils.ResourceLoader/js/resourceloader.js";
    private const string _moduleName = "ResourceLoader";

    public ResourceLoader(IJSRuntime jsRuntime, IModuleImportUtil moduleImportUtil, IJsVariableInterop jsVariableInterop)
    {
        _moduleImportUtil = moduleImportUtil;
        _jsVariableInterop = jsVariableInterop;

        _initializer = new AsyncSingleton(async (token, _) =>
        {
            await _moduleImportUtil.ImportAndWaitUntilAvailable(_modulePath, _moduleName, 100, token).NoSync();

            return new object();
        });

        _scripts = new SingletonDictionary<object>(async (uri, token, objects) =>
        {
            var integrity = (string?)objects[0];
            var crossOrigin = (string?)objects[1];
            var loadInHead = (bool)objects[2];
            var async = (bool)objects[3];
            var defer = (bool)objects[4];

            await jsRuntime.InvokeVoidAsync($"{_moduleName}.loadScript", token, uri, integrity, crossOrigin, loadInHead, async, defer).NoSync();

            return new object();
        });

        _styles = new SingletonDictionary<object>(async (uri, token, objects) =>
        {
            var integrity = (string?) objects[0];
            var crossOrigin = (string?)objects[1];
            var media = (string?) objects[2];
            var type = (string?) objects[3];

            await jsRuntime.InvokeVoidAsync($"{_moduleName}.loadStyle", token, uri, integrity, crossOrigin, media, type).NoSync();

            return new object();
        });
    }

    public async ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false, bool defer = false,
        CancellationToken cancellationToken = default)
    {
        await _initializer.Init(cancellationToken).NoSync();
        _ = await _scripts.Get(uri, cancellationToken, integrity!, crossOrigin!, loadInHead, async, defer).NoSync();
    }

    public async ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, CancellationToken cancellationToken = default)
    {
        await LoadScript(uri, integrity, crossOrigin, loadInHead, async, defer, cancellationToken).NoSync();
        await WaitForVariable(variableName, cancellationToken: cancellationToken).NoSync();
    }

    public async ValueTask LoadStyle(string uri, string? integrity, string? crossOrigin = "anonymous", string? media = "all", string? type = "text/css", CancellationToken cancellationToken = default)
    {
        await _initializer.Init(cancellationToken).NoSync();
        _ = await _styles.Get(uri, cancellationToken, integrity!, crossOrigin!, media!, type!).NoSync();
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

    public async ValueTask DisposeAsync()
    {
        await _moduleImportUtil.DisposeModule(_modulePath).NoSync();

        await _scripts.DisposeAsync().NoSync();
        await _styles.DisposeAsync().NoSync();

        await _initializer.DisposeAsync().NoSync();
    }
}
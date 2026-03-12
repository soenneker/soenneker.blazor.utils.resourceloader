using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using System.Threading.Tasks;
using System.Threading;
using Soenneker.Asyncs.Initializers;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.Utils.JsVariable.Abstract;
using Soenneker.Dictionaries.Singletons;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.CancellationScopes;

namespace Soenneker.Blazor.Utils.ResourceLoader;

///<inheritdoc cref="IResourceLoader"/>
public sealed class ResourceLoader : IResourceLoader
{
    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly IJsVariableInterop _jsVariableInterop;
    private readonly IJSRuntime _jsRuntime;

    private readonly SingletonDictionary<object, ScriptLoadArgs> _scripts;
    private readonly SingletonDictionary<object, StyleLoadArgs> _styles;

    private readonly AsyncInitializer _initializer;

    private const string _modulePath = "Soenneker.Blazor.Utils.ResourceLoader/js/resourceloader.js";
    private const string _moduleName = "ResourceLoader";

    private readonly record struct ScriptLoadArgs(string? Integrity, string? CrossOrigin, bool LoadInHead, bool Async, bool Defer);
    private readonly record struct StyleLoadArgs(string? Integrity, string? CrossOrigin, string? Media, string? Type);

    private readonly CancellationScope _cancellationScope = new();

    public ResourceLoader(IJSRuntime jsRuntime, IModuleImportUtil moduleImportUtil, IJsVariableInterop jsVariableInterop)
    {
        _moduleImportUtil = moduleImportUtil;
        _jsVariableInterop = jsVariableInterop;
        _jsRuntime = jsRuntime;

        _initializer = new AsyncInitializer(Initialize);

        _scripts = new SingletonDictionary<object, ScriptLoadArgs>(LoadScript);
        _styles = new SingletonDictionary<object, StyleLoadArgs>(LoadStyle);
    }

    private ValueTask Initialize(CancellationToken token)
    {
        return _moduleImportUtil.ImportAndWaitUntilAvailable(_modulePath, _moduleName, 100, token);
    }

    private async ValueTask<object> LoadScript(string uri, ScriptLoadArgs args, CancellationToken token)
    {
        await _jsRuntime.InvokeVoidAsync("ResourceLoader.loadScript", token, uri, args.Integrity, args.CrossOrigin, args.LoadInHead, args.Async, args.Defer);

        return new object();
    }

    private async ValueTask<object> LoadStyle(string uri, StyleLoadArgs args, CancellationToken token)
    {
        await _jsRuntime.InvokeVoidAsync("ResourceLoader.loadStyle", token, uri, args.Integrity, args.CrossOrigin, args.Media, args.Type);

        return new object();
    }

    public async ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
        {
            await _initializer.Init(linked);

            var args = new ScriptLoadArgs(Integrity: integrity, CrossOrigin: crossOrigin, LoadInHead: loadInHead, Async: async, Defer: defer);

            _ = await _scripts.Get(uri, args, linked);
        }
    }


    public async ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, bool async = false, bool defer = false, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
        {
            await LoadScript(uri, integrity, crossOrigin, loadInHead, async, defer, linked);
            await WaitForVariable(variableName, cancellationToken: linked);
        }
    }

    public async ValueTask LoadStyle(string uri, string? integrity, string? crossOrigin = "anonymous", string? media = "all", string? type = "text/css",
        CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
        {
            await _initializer.Init(linked);

            var args = new StyleLoadArgs(Integrity: integrity, CrossOrigin: crossOrigin, Media: media, Type: type);

            _ = await _styles.Get(uri, args, linked);
        }
    }


    public async ValueTask<IJSObjectReference> ImportModule(string name, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
            return await _moduleImportUtil.Import(name, linked);
    }

    public async ValueTask ImportModuleAndWait(string name, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
            await _moduleImportUtil.ImportAndWait(name, linked);
    }

    public async ValueTask ImportModuleAndWaitUntilAvailable(string name, string variableName, int delay = 100, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
            await _moduleImportUtil.ImportAndWaitUntilAvailable(name, variableName, delay, linked);
    }

    public async ValueTask WaitForVariable(string variableName, int delay = 100, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
            await _jsVariableInterop.WaitForVariable(variableName, delay, linked);
    }

    public async ValueTask DisposeModule(string name, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
            await _moduleImportUtil.DisposeModule(name, linked);
    }

    public async ValueTask DisposeAsync()
    {
        await _moduleImportUtil.DisposeModule(_modulePath);

        await _scripts.DisposeAsync();
        await _styles.DisposeAsync();

        await _initializer.DisposeAsync();
        await _cancellationScope.DisposeAsync();
    }
}
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using System.Threading.Tasks;
using System.Threading;
using Soenneker.Asyncs.Initializers;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.Utils.JsVariable.Abstract;
using Soenneker.Utils.SingletonDictionary;

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

    private async ValueTask<object> LoadScript(string uri, CancellationToken token, ScriptLoadArgs args)
    {
        await _jsRuntime.InvokeVoidAsync("ResourceLoaderInterop.loadScript", token, uri, args.Integrity, args.CrossOrigin, args.LoadInHead, args.Async, args.Defer);

        return new object();
    }

    private async ValueTask<object> LoadStyle(string uri, CancellationToken token, StyleLoadArgs args)
    {
        await _jsRuntime.InvokeVoidAsync("ResourceLoaderInterop.loadStyle", token, uri, args.Integrity, args.CrossOrigin, args.Media, args.Type);

        return new object();
    }

    public async ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, CancellationToken cancellationToken = default)
    {
        await _initializer.Init(cancellationToken);

        var args = new ScriptLoadArgs(Integrity: integrity, CrossOrigin: crossOrigin, LoadInHead: loadInHead, Async: async, Defer: defer);

        _ = await _scripts.Get(uri, args, cancellationToken);
    }


    public async ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, bool async = false, bool defer = false, CancellationToken cancellationToken = default)
    {
        await LoadScript(uri, integrity, crossOrigin, loadInHead, async, defer, cancellationToken);
        await WaitForVariable(variableName, cancellationToken: cancellationToken);
    }

    public async ValueTask LoadStyle(string uri, string? integrity, string? crossOrigin = "anonymous", string? media = "all", string? type = "text/css",
        CancellationToken cancellationToken = default)
    {
        await _initializer.Init(cancellationToken);

        var args = new StyleLoadArgs(Integrity: integrity, CrossOrigin: crossOrigin, Media: media, Type: type);

        _ = await _styles.Get(uri, args, cancellationToken);
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
        await _moduleImportUtil.DisposeModule(_modulePath);

        await _scripts.DisposeAsync();
        await _styles.DisposeAsync();

        await _initializer.DisposeAsync();
    }
}
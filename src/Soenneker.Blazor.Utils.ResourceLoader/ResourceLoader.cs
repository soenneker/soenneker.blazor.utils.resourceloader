using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using System.Threading.Tasks;
using System.Threading;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.Utils.JsVariable.Abstract;
using Soenneker.Dictionaries.Singletons;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.CancellationScopes;
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ResourceLoader.Dtos;

namespace Soenneker.Blazor.Utils.ResourceLoader;

///<inheritdoc cref="IResourceLoader"/>
public sealed class ResourceLoader : IResourceLoader
{
    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly IJsVariableInterop _jsVariableInterop;

    private readonly SingletonDictionary<object, ScriptLoadArgs> _scripts;
    private readonly SingletonDictionary<object, StyleLoadArgs> _styles;

    private const string _modulePath = "Soenneker.Blazor.Utils.ResourceLoader/js/resourceloader.js";

    private readonly CancellationScope _cancellationScope = new();

    public ResourceLoader(IModuleImportUtil moduleImportUtil, IJsVariableInterop jsVariableInterop)
    {
        _moduleImportUtil = moduleImportUtil;
        _jsVariableInterop = jsVariableInterop;

        _scripts = new SingletonDictionary<object, ScriptLoadArgs>(LoadScript);
        _styles = new SingletonDictionary<object, StyleLoadArgs>(LoadStyle);
    }

    private ValueTask<IJSObjectReference> GetResourceLoaderModule(CancellationToken cancellationToken)
    {
        return _moduleImportUtil.Import(_modulePath, cancellationToken);
    }

    private async ValueTask<object> LoadScript(string uri, ScriptLoadArgs args, CancellationToken token)
    {
        IJSObjectReference module = await GetResourceLoaderModule(token);
        await module.InvokeVoidAsync("loadScript", token, uri, args.Integrity, args.CrossOrigin, args.LoadInHead, args.Async, args.Defer);

        return new object();
    }

    private async ValueTask<object> LoadStyle(string uri, StyleLoadArgs args, CancellationToken token)
    {
        IJSObjectReference module = await GetResourceLoaderModule(token);
        await module.InvokeVoidAsync("loadStyle", token, uri, args.Integrity, args.CrossOrigin, args.Media, args.Type);

        return new object();
    }

    public async ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
            await LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, async, defer, linked);
    }


    public async ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, bool async = false, bool defer = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, async, defer, linked);
            await WaitForVariableInternal(variableName, delay, timeout, linked);
        }
    }

    public async ValueTask LoadStyle(string uri, string? integrity, string? crossOrigin = "anonymous", string? media = "all", string? type = "text/css",
        CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
            await LoadStyleInternal(uri, integrity, crossOrigin, media, type, linked);
    }


    public async ValueTask<IJSObjectReference> ImportModule(string name, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
            return await _moduleImportUtil.Import(name, linked);
    }

    public async ValueTask ImportModuleAndWait(string name, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
            _ = await _moduleImportUtil.Import(name, linked);
    }

    public async ValueTask WaitForVariable(string variableName, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
            await WaitForVariableInternal(variableName, delay, timeout, linked);
    }

    public async ValueTask DisposeModule(string name, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
            await _moduleImportUtil.DisposeModule(name, linked);
    }

    private async ValueTask LoadScriptInternal(string uri, string? integrity, string? crossOrigin, bool loadInHead, bool async, bool defer,
        CancellationToken cancellationToken)
    {
        var args = new ScriptLoadArgs(Integrity: integrity, CrossOrigin: crossOrigin, LoadInHead: loadInHead, Async: async, Defer: defer);
        _ = await _scripts.Get(uri, args, cancellationToken);
    }

    private async ValueTask LoadStyleInternal(string uri, string? integrity, string? crossOrigin, string? media, string? type, CancellationToken cancellationToken)
    {
        var args = new StyleLoadArgs(Integrity: integrity, CrossOrigin: crossOrigin, Media: media, Type: type);
        _ = await _styles.Get(uri, args, cancellationToken);
    }

    private ValueTask WaitForVariableInternal(string variableName, int delay, int? timeout, CancellationToken cancellationToken)
    {
        return _jsVariableInterop.WaitForVariable(variableName, delay, timeout, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _moduleImportUtil.DisposeModule(_modulePath);

        await _scripts.DisposeAsync();
        await _styles.DisposeAsync();
        await _cancellationScope.DisposeAsync();
    }
}
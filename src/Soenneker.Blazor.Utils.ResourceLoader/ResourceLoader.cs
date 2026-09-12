using Soenneker.Asyncs.Locks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.JsVariable.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Dtos;
using Soenneker.Atomics.ValueBools;
using Soenneker.Extensions.CancellationTokens;

namespace Soenneker.Blazor.Utils.ResourceLoader;

public sealed class ResourceLoader : IResourceLoader
{
    private const string _modulePath = "./_content/Soenneker.Blazor.Utils.ResourceLoader/js/resourceloader.js";

    private static readonly object _true = true;
    private static readonly object _false = false;

    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly IJsVariableInterop _jsVariableInterop;
    private ResourceCache<ScriptLoadArgs>? _scripts;
    private ResourceCache<StyleLoadArgs>? _styles;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private readonly CancellationToken _lifetimeToken;
    private ValueAtomicBool _disposed;
    private readonly AsyncLock _lifetimeGate = new();

    public ResourceLoader(IModuleImportUtil moduleImportUtil, IJsVariableInterop jsVariableInterop)
    {
        _lifetimeToken = _lifetimeCancellation.Token;
        _moduleImportUtil = moduleImportUtil ?? throw new ArgumentNullException(nameof(moduleImportUtil));
        _jsVariableInterop = jsVariableInterop ?? throw new ArgumentNullException(nameof(jsVariableInterop));

    }

    private async ValueTask LoadScriptCore(ScriptLoadArgs args, CancellationToken cancellationToken)
    {
        IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, cancellationToken);
        await module.InvokeVoidAsync("loadScript", cancellationToken, args.Uri, args.Integrity, args.CrossOrigin,
            args.LoadInHead ? _true : _false, args.Async ? _true : _false, args.Defer ? _true : _false, args.IsModule ? _true : _false);
    }

    private async ValueTask LoadStyleCore(StyleLoadArgs args, CancellationToken cancellationToken)
    {
        IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, cancellationToken);
        await module.InvokeVoidAsync("loadStyle", cancellationToken, args.Uri, args.Integrity, args.CrossOrigin, args.Media, args.Type);
    }

    public ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, CancellationToken cancellationToken = default)
    {
        return LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, async, defer, false, cancellationToken);
    }

    public ValueTask LoadModuleScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false,
        CancellationToken cancellationToken = default)
    {
        return LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, false, false, true, cancellationToken);
    }

    public async ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, bool async = false, bool defer = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default)
    {
        await LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, async, defer, false, cancellationToken);
        await WaitForVariable(variableName, delay, timeout, cancellationToken);
    }

    public async ValueTask LoadModuleScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default)
    {
        await LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, false, false, true, cancellationToken);
        await WaitForVariable(variableName, delay, timeout, cancellationToken);
    }

    public ValueTask LoadStyle(string uri, string? integrity = null, string? crossOrigin = "anonymous", string? media = "all", string? type = "text/css",
        CancellationToken cancellationToken = default)
    {
        return LoadStyleInternal(uri, integrity, crossOrigin, media, type, cancellationToken);
    }

    public async ValueTask WaitForVariable(string variableName, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        CancellationToken linked = GetLifetimeToken().Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await _jsVariableInterop.WaitForVariable(variableName, delay, timeout, linked);
        }
    }

    private ValueTask LoadScriptInternal(string uri, string? integrity, string? crossOrigin, bool loadInHead, bool async, bool defer, bool isModule,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ValidateUri(uri);
        ValidateCrossOrigin(crossOrigin);

        var args = new ScriptLoadArgs(Uri: uri, Integrity: integrity, CrossOrigin: crossOrigin, LoadInHead: loadInHead, Async: async, Defer: defer,
            IsModule: isModule);

        ResourceCache<ScriptLoadArgs> scripts = Volatile.Read(ref _scripts) ?? CreateScripts();
        return scripts.IsLoaded(args) ? ValueTask.CompletedTask : LoadUncached(scripts, args, cancellationToken);
    }

    private ValueTask LoadStyleInternal(string uri, string? integrity, string? crossOrigin, string? media, string? type,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ValidateUri(uri);
        ValidateCrossOrigin(crossOrigin);

        var args = new StyleLoadArgs(Uri: uri, Integrity: integrity, CrossOrigin: crossOrigin, Media: media, Type: type);

        ResourceCache<StyleLoadArgs> styles = Volatile.Read(ref _styles) ?? CreateStyles();
        return styles.IsLoaded(args) ? ValueTask.CompletedTask : LoadUncached(styles, args, cancellationToken);
    }

    private ResourceCache<ScriptLoadArgs> CreateScripts()
    {
        using (_lifetimeGate.LockSync())
        {
            ThrowIfDisposed();
            return _scripts ??= new ResourceCache<ScriptLoadArgs>(LoadScriptCore);
        }
    }

    private ResourceCache<StyleLoadArgs> CreateStyles()
    {
        using (_lifetimeGate.LockSync())
        {
            ThrowIfDisposed();
            return _styles ??= new ResourceCache<StyleLoadArgs>(LoadStyleCore);
        }
    }

    private async ValueTask LoadUncached<TArgs>(ResourceCache<TArgs> cache, TArgs args, CancellationToken cancellationToken)
        where TArgs : notnull
    {
        ThrowIfDisposed();
        CancellationToken linked = GetLifetimeToken().Link(cancellationToken, out CancellationTokenSource? source);
        using (source)
            await cache.Get(args, linked);
    }

    private static void ValidateUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("A resource URL is required.", nameof(uri));
    }

    private static void ValidateCrossOrigin(string? crossOrigin)
    {
        if (string.IsNullOrEmpty(crossOrigin) || crossOrigin is "anonymous" or "use-credentials")
            return;

        throw new ArgumentException("The CORS mode must be null, empty, 'anonymous', or 'use-credentials'.", nameof(crossOrigin));
    }

    private void ThrowIfDisposed()
    {
        if (_disposed.Value)
            throw new ObjectDisposedException(nameof(ResourceLoader));
    }

    private CancellationToken GetLifetimeToken()
    {
        using (_lifetimeGate.LockSync())
        {
            ObjectDisposedException.ThrowIf(_disposed.Value, this);
            return _lifetimeToken;
        }
    }

    private async ValueTask CancelLifetime()
    {
        try
        {
            await _lifetimeCancellation.CancelAsync().ConfigureAwait(false);
        }
        catch
        {
            // A cancellation callback must not prevent reference cleanup.
        }
        finally
        {
            _lifetimeCancellation.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        using (await _lifetimeGate.Lock().ConfigureAwait(false))
        {
            if (!_disposed.TrySetTrue())
                return;
        }

        await CancelLifetime();
        if (_scripts is not null)
            await _scripts.DisposeAsync().ConfigureAwait(false);
        if (_styles is not null)
            await _styles.DisposeAsync().ConfigureAwait(false);
        if (_scripts is not null || _styles is not null)
            await _moduleImportUtil.DisposeContentModule(_modulePath).ConfigureAwait(false);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.JsVariable.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Dtos;
using Soenneker.Atomics.ValueBools;
using Soenneker.Dictionaries.Singletons;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.CancellationScopes;
using System.Text.Json;

namespace Soenneker.Blazor.Utils.ResourceLoader;

/// <inheritdoc cref="IResourceLoader"/>
public sealed class ResourceLoader : IResourceLoader
{
    private const string _modulePath = "./_content/Soenneker.Blazor.Utils.ResourceLoader/js/resourceloader.js";

    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly IJsVariableInterop _jsVariableInterop;
    private readonly SingletonDictionary<ResourceLoadItem, ScriptLoadArgs> _scripts;
    private readonly SingletonDictionary<ResourceLoadItem, StyleLoadArgs> _styles;
    private readonly CancellationScope _cancellationScope = new();
    private ValueAtomicBool _disposed;

    public ResourceLoader(IModuleImportUtil moduleImportUtil, IJsVariableInterop jsVariableInterop)
    {
        _moduleImportUtil = moduleImportUtil ?? throw new ArgumentNullException(nameof(moduleImportUtil));
        _jsVariableInterop = jsVariableInterop ?? throw new ArgumentNullException(nameof(jsVariableInterop));

        _scripts = new SingletonDictionary<ResourceLoadItem, ScriptLoadArgs>(LoadScriptCore);
        _styles = new SingletonDictionary<ResourceLoadItem, StyleLoadArgs>(LoadStyleCore);
    }

    private async ValueTask<ResourceLoadItem> LoadScriptCore(string _, ScriptLoadArgs args, CancellationToken cancellationToken)
    {
        var item = new ResourceLoadItem();

        try
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, cancellationToken);

            await module.InvokeVoidAsync("loadScript", cancellationToken, args.Uri, args.Integrity, args.CrossOrigin, args.LoadInHead, args.Async, args.Defer,
                args.IsModule);

            item.LoadedTcs.TrySetResult(true);
            return item;
        }
        catch (OperationCanceledException ex)
        {
            item.LoadedTcs.TrySetCanceled(ex.CancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            item.LoadedTcs.TrySetException(ex);
            throw;
        }
    }

    private async ValueTask<ResourceLoadItem> LoadStyleCore(string _, StyleLoadArgs args, CancellationToken cancellationToken)
    {
        var item = new ResourceLoadItem();

        try
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, cancellationToken);

            await module.InvokeVoidAsync("loadStyle", cancellationToken, args.Uri, args.Integrity, args.CrossOrigin, args.Media, args.Type);

            item.LoadedTcs.TrySetResult(true);
            return item;
        }
        catch (OperationCanceledException ex)
        {
            item.LoadedTcs.TrySetCanceled(ex.CancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            item.LoadedTcs.TrySetException(ex);
            throw;
        }
    }

    public async ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, async, defer, false, linked);
        }
    }

    public async ValueTask LoadModuleScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false,
        CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, false, false, true, linked);
        }
    }

    public async ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, bool async = false, bool defer = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, async, defer, false, linked);
            await _jsVariableInterop.WaitForVariable(variableName, delay, timeout, linked);
        }
    }

    public async ValueTask LoadModuleScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await LoadScriptInternal(uri, integrity, crossOrigin, loadInHead, false, false, true, linked);
            await _jsVariableInterop.WaitForVariable(variableName, delay, timeout, linked);
        }
    }

    public async ValueTask LoadStyle(string uri, string? integrity = null, string? crossOrigin = "anonymous", string? media = "all", string? type = "text/css",
        CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await LoadStyleInternal(uri, integrity, crossOrigin, media, type, linked);
        }
    }

    public async ValueTask WaitForVariable(string variableName, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await _jsVariableInterop.WaitForVariable(variableName, delay, timeout, linked);
        }
    }

    private async ValueTask LoadScriptInternal(string uri, string? integrity, string? crossOrigin, bool loadInHead, bool async, bool defer, bool isModule,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ValidateUri(uri);
        ValidateCrossOrigin(crossOrigin);

        var args = new ScriptLoadArgs(Uri: uri, Integrity: integrity, CrossOrigin: crossOrigin, LoadInHead: loadInHead, Async: async, Defer: defer,
            IsModule: isModule);
        string key = JsonSerializer.Serialize(args);

        ResourceLoadItem item = await _scripts.Get(key, args, cancellationToken);
        await item.Loaded.WaitAsync(cancellationToken);
    }

    private async ValueTask LoadStyleInternal(string uri, string? integrity, string? crossOrigin, string? media, string? type,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ValidateUri(uri);
        ValidateCrossOrigin(crossOrigin);

        var args = new StyleLoadArgs(Uri: uri, Integrity: integrity, CrossOrigin: crossOrigin, Media: media, Type: type);
        string key = JsonSerializer.Serialize(args);

        ResourceLoadItem item = await _styles.Get(key, args, cancellationToken);
        await item.Loaded.WaitAsync(cancellationToken);
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

    /// <summary>
    /// Asynchronously releases resources used by the current instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed.TrySetTrue())
            return;

        await _cancellationScope.DisposeAsync().NoSync();
        await _scripts.DisposeAsync().NoSync();
        await _styles.DisposeAsync().NoSync();
        await _moduleImportUtil.DisposeContentModule(_modulePath).NoSync();
    }
}

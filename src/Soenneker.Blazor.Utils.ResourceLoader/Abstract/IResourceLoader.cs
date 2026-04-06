using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.JSInterop;

namespace Soenneker.Blazor.Utils.ResourceLoader.Abstract;

/// <summary>
/// A Blazor JavaScript interop for dynamically loading scripts, styles, and modules
/// Ensures that each resource is only loaded once (through this interop), even with multiple concurrent calls.
/// </summary>
public interface IResourceLoader : IAsyncDisposable
{
    /// <summary>
    /// Loads a script from the specified URI if it hasn't already been loaded (through the ResourceLoader)
    /// </summary>
    /// <param name="uri">The URI of the script to load.</param>
    /// <param name="integrity">The integrity hash of the script for Subresource Integrity (SRI) validation.</param>
    /// <param name="defer"></param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="crossOrigin"></param>
    /// <param name="loadInHead"></param>
    /// <param name="async"></param>
    /// <returns>A task that represents the asynchronous operation. The task will complete when the script is loaded or if it has already been loaded.</returns>
    ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false, bool defer = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an ES module script tag from the specified URI if it hasn't already been loaded.
    /// </summary>
    /// <param name="uri">The URI of the module script to load.</param>
    /// <param name="integrity">Optional integrity hash for static module assets.</param>
    /// <param name="crossOrigin">The crossorigin mode to apply to the script tag.</param>
    /// <param name="loadInHead">If true, appends the script to the document head; otherwise appends to the body.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    ValueTask LoadModuleScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a script from the specified URI and waits until the specified JavaScript variable is available.
    /// </summary>
    /// <param name="uri">The URI of the script to load.</param>
    /// <param name="variableName">The name of the JavaScript variable to wait for.</param>
    /// <param name="integrity">The integrity hash of the script for Subresource Integrity (SRI) validation. This parameter is optional.</param>
    /// <param name="defer"></param>
    /// <param name="delay">The delay in milliseconds between each fallback availability check. The default is 16 milliseconds.</param>
    /// <param name="timeout">An optional timeout in milliseconds for the fallback wait.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="crossOrigin"></param>
    /// <param name="loadInHead"></param>
    /// <param name="async"></param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the script is loaded and the variable is available or the operation is cancelled.</returns>
    ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an ES module script and waits until the specified JavaScript global becomes available.
    /// </summary>
    ValueTask LoadModuleScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a style from the specified URI if it hasn't already been loaded  (through the ResourceLoader)
    /// </summary>
    /// <param name="uri">The URI of the style to load.</param>
    /// <param name="integrity">The integrity hash of the style for Subresource Integrity (SRI) validation.</param>
    /// <param name="type"></param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="crossOrigin"></param>
    /// <param name="media"></param>
    /// <returns>A task that represents the asynchronous operation. The task will complete when the style is loaded or if it has already been loaded.</returns>
    ValueTask LoadStyle(string uri, string? integrity = null, string? crossOrigin = "anonymous", string? media = "all", string? type = "text/css", CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously waits until a specified JavaScript variable is available in the global scope.
    /// </summary>
    /// <param name="variableName">The name of the JavaScript variable to wait for.</param>
    /// <param name="delay">The delay in milliseconds between each availability check. The default is 16 milliseconds.</param>
    /// <param name="timeout">An optional timeout in milliseconds. If specified, the operation throws when the timeout elapses before the variable becomes available.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
    /// <remarks>This method ensures the necessary JavaScript is injected and repeatedly checks for the variable's availability until it becomes available or the operation is canceled.</remarks>
    ValueTask WaitForVariable(string variableName, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default);
}

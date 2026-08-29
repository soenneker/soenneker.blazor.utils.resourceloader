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
    /// <param name="uri">Receives the normalized absolute URI when parsing succeeds.</param>
    /// <param name="integrity">The integrity hash of the script for Subresource Integrity (SRI) validation.</param>
    /// <param name="crossOrigin">CORS mode assigned to the script element.</param>
    /// <param name="loadInHead">Whether load in head.</param>
    /// <param name="async">Whether async.</param>
    /// <param name="defer">Whether defer.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the script has been loaded.</returns>
    ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false, bool defer = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an ES module script tag from the specified URI if it hasn't already been loaded.
    /// </summary>
    /// <param name="uri">Receives the normalized absolute URI when parsing succeeds.</param>
    /// <param name="integrity">Optional integrity hash for static module assets.</param>
    /// <param name="crossOrigin">The crossorigin mode to apply to the script tag.</param>
    /// <param name="loadInHead">If true, appends the script to the document head; otherwise appends to the body.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the module script has been loaded.</returns>
    ValueTask LoadModuleScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a script from the specified URI and waits until the specified JavaScript variable is available.
    /// </summary>
    /// <param name="uri">Receives the normalized absolute URI when parsing succeeds.</param>
    /// <param name="variableName">The name of the JavaScript variable to wait for.</param>
    /// <param name="integrity">The integrity hash of the script for Subresource Integrity (SRI) validation. This parameter is optional.</param>
    /// <param name="crossOrigin">CORS mode assigned to the script element.</param>
    /// <param name="loadInHead">Whether load in head.</param>
    /// <param name="async">Whether async.</param>
    /// <param name="defer">Whether defer.</param>
    /// <param name="delay">The delay in milliseconds between each fallback availability check. The default is 16 milliseconds.</param>
    /// <param name="timeout">An optional timeout in milliseconds for the fallback wait.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the script and wait for variable has been loaded.</returns>
    ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an ES module script and waits until the specified JavaScript global becomes available.
    /// </summary>
    /// <param name="uri">Receives the normalized absolute URI when parsing succeeds.</param>
    /// <param name="variableName">Name of the variable to target.</param>
    /// <param name="integrity">Integrity for the load module script and wait for variable operation.</param>
    /// <param name="crossOrigin">CORS mode assigned to the script element.</param>
    /// <param name="loadInHead">Whether load in head.</param>
    /// <param name="delay">Delay to apply before continuing.</param>
    /// <param name="timeout">Maximum time to wait before the operation is abandoned.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the module script and wait for variable has been loaded.</returns>
    ValueTask LoadModuleScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a style from the specified URI if it hasn't already been loaded  (through the ResourceLoader)
    /// </summary>
    /// <param name="uri">Receives the normalized absolute URI when parsing succeeds.</param>
    /// <param name="integrity">The integrity hash of the style for Subresource Integrity (SRI) validation.</param>
    /// <param name="crossOrigin">CORS mode assigned to the script element.</param>
    /// <param name="media">Media for the load style operation.</param>
    /// <param name="type">Runtime type to inspect or construct.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the style has been loaded.</returns>
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

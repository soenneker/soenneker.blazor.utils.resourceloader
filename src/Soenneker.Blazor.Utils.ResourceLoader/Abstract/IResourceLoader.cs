using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.JSInterop;

namespace Soenneker.Blazor.Utils.ResourceLoader.Abstract;

/// <summary>
/// Loads JavaScript and stylesheet resources into a Blazor application's document and coalesces concurrent requests with identical options.
/// </summary>
public interface IResourceLoader : IAsyncDisposable
{
    /// <summary>
    /// Loads a classic script element and waits for its load event.
    /// </summary>
    /// <param name="uri">A relative or absolute HTTP(S) URL.</param>
    /// <param name="integrity">An optional Subresource Integrity hash.</param>
    /// <param name="crossOrigin">The element's CORS mode: <c>anonymous</c>, <c>use-credentials</c>, or no value.</param>
    /// <param name="loadInHead">Whether to append the element to the document head instead of the body.</param>
    /// <param name="async">Whether to set the script's <c>async</c> property.</param>
    /// <param name="defer">Whether to set the script's <c>defer</c> property.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the script has been loaded.</returns>
    ValueTask LoadScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false, bool defer = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an ES module script element and waits for its load event.
    /// </summary>
    /// <param name="uri">A relative or absolute HTTP(S) URL.</param>
    /// <param name="integrity">An optional Subresource Integrity hash.</param>
    /// <param name="crossOrigin">The element's CORS mode: <c>anonymous</c>, <c>use-credentials</c>, or no value.</param>
    /// <param name="loadInHead">If true, appends the script to the document head; otherwise appends to the body.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the module script has been loaded.</returns>
    ValueTask LoadModuleScript(string uri, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a script from the specified URI and waits until the specified JavaScript variable is available.
    /// </summary>
    /// <param name="uri">A relative or absolute HTTP(S) URL.</param>
    /// <param name="variableName">The dotted path of the global JavaScript variable to wait for.</param>
    /// <param name="integrity">An optional Subresource Integrity hash.</param>
    /// <param name="crossOrigin">The element's CORS mode: <c>anonymous</c>, <c>use-credentials</c>, or no value.</param>
    /// <param name="loadInHead">Whether to append the element to the document head instead of the body.</param>
    /// <param name="async">Whether to set the script's <c>async</c> property.</param>
    /// <param name="defer">Whether to set the script's <c>defer</c> property.</param>
    /// <param name="delay">The delay in milliseconds between availability checks.</param>
    /// <param name="timeout">An optional timeout in milliseconds.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes after the script loads and the variable becomes available.</returns>
    ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous", bool loadInHead = false, bool async = false,
        bool defer = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an ES module script and waits until the specified JavaScript global becomes available.
    /// </summary>
    /// <param name="uri">A relative or absolute HTTP(S) URL.</param>
    /// <param name="variableName">The dotted path of the global JavaScript variable to wait for.</param>
    /// <param name="integrity">An optional Subresource Integrity hash.</param>
    /// <param name="crossOrigin">The element's CORS mode: <c>anonymous</c>, <c>use-credentials</c>, or no value.</param>
    /// <param name="loadInHead">Whether to append the element to the document head instead of the body.</param>
    /// <param name="delay">The delay in milliseconds between availability checks.</param>
    /// <param name="timeout">An optional timeout in milliseconds.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes after the module script loads and the variable becomes available.</returns>
    ValueTask LoadModuleScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, string? crossOrigin = "anonymous",
        bool loadInHead = false, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a stylesheet link element and waits for its load event.
    /// </summary>
    /// <param name="uri">A relative or absolute HTTP(S) URL.</param>
    /// <param name="integrity">An optional Subresource Integrity hash.</param>
    /// <param name="crossOrigin">The element's CORS mode: <c>anonymous</c>, <c>use-credentials</c>, or no value.</param>
    /// <param name="media">The stylesheet's media query.</param>
    /// <param name="type">The stylesheet MIME type.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the style has been loaded.</returns>
    ValueTask LoadStyle(string uri, string? integrity = null, string? crossOrigin = "anonymous", string? media = "all", string? type = "text/css", CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously waits until a specified JavaScript variable is available in the global scope.
    /// </summary>
    /// <param name="variableName">The dotted path of the global JavaScript variable to wait for.</param>
    /// <param name="delay">The delay in milliseconds between each availability check. The default is 16 milliseconds.</param>
    /// <param name="timeout">An optional timeout in milliseconds. If specified, the operation throws when the timeout elapses before the variable becomes available.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
    /// <remarks>This method does not load a script. It only waits for an existing global variable.</remarks>
    ValueTask WaitForVariable(string variableName, int delay = 16, int? timeout = null, CancellationToken cancellationToken = default);
}

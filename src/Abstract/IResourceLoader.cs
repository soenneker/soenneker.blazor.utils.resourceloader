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
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task will complete when the script is loaded or if it has already been loaded.</returns>
    ValueTask LoadScript(string uri, string? integrity = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a script from the specified URI and waits until the specified JavaScript variable is available.
    /// </summary>
    /// <param name="uri">The URI of the script to load.</param>
    /// <param name="variableName">The name of the JavaScript variable to wait for.</param>
    /// <param name="integrity">The integrity hash of the script for Subresource Integrity (SRI) validation. This parameter is optional.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the script is loaded and the variable is available or the operation is cancelled.</returns>
    ValueTask LoadScriptAndWaitForVariable(string uri, string variableName, string? integrity = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports a JavaScript module by its name.
    /// </summary>
    /// <param name="name">The name of the JavaScript module to import.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the imported JavaScript module reference.</returns>
    ValueTask<IJSObjectReference> ImportModule(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits until the specified module is loaded.
    /// </summary>
    /// <param name="name">The name of the JavaScript module.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask ImportModuleAndWait(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits until the specified JavaScript module is loaded and JS variable is available (not undefined).
    /// </summary>
    /// <param name="name">The name of the script or style to wait for.</param>
    /// <param name="variableName">The name of the JavaScript variable to check for availability.</param>
    /// <param name="delay">The delay in milliseconds between each check for the variable's availability. The default is 100 milliseconds.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the variable is available or the operation is cancelled.</returns>
    ValueTask ImportModuleAndWaitUntilAvailable(string name, string variableName, int delay = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a style from the specified URI if it hasn't already been loaded  (through the ResourceLoader)
    /// </summary>
    /// <param name="uri">The URI of the style to load.</param>
    /// <param name="integrity">The integrity hash of the style for Subresource Integrity (SRI) validation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task will complete when the style is loaded or if it has already been loaded.</returns>
    ValueTask LoadStyle(string uri, string? integrity = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously waits until a specified JavaScript variable is available in the global scope.
    /// </summary>
    /// <param name="variableName">The name of the JavaScript variable to wait for.</param>
    /// <param name="delay">The delay in milliseconds between each availability check. The default is 100 milliseconds.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
    /// <remarks>This method ensures the necessary JavaScript is injected and repeatedly checks for the variable's availability until it becomes available or the operation is canceled.</remarks>
    ValueTask WaitForVariable(string variableName, int delay = 100, CancellationToken cancellationToken = default);

    public ValueTask DisposeModule(string name, CancellationToken cancellationToken = default);
}
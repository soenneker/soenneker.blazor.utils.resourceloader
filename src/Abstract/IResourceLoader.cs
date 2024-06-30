using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Blazor.Utils.ResourceLoader.Abstract;

/// <summary>
/// Defines methods for loading external resources such as scripts and styles. 
/// Ensures that each resource is only loaded once, even with multiple concurrent calls.
/// </summary>
public interface IResourceLoader
{
    /// <summary>
    /// Loads a script from the specified URI if it hasn't already been loaded (through the ResourceLoader)
    /// </summary>
    /// <param name="uri">The URI of the script to load.</param>
    /// <param name="integrity">The integrity hash of the script for Subresource Integrity (SRI) validation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task will complete when the script is loaded or if it has already been loaded.</returns>
    ValueTask LoadScript(string uri, string integrity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a style from the specified URI if it hasn't already been loaded  (through the ResourceLoader)
    /// </summary>
    /// <param name="uri">The URI of the style to load.</param>
    /// <param name="integrity">The integrity hash of the style for Subresource Integrity (SRI) validation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task will complete when the style is loaded or if it has already been loaded.</returns>
    ValueTask LoadStyle(string uri, string integrity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously waits until a specified JavaScript variable is available in the global scope.
    /// </summary>
    /// <param name="variableName">The name of the JavaScript variable to wait for.</param>
    /// <param name="delay">The delay in milliseconds between each availability check. The default is 100 milliseconds.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
    /// <remarks>This method ensures the necessary JavaScript is injected and repeatedly checks for the variable's availability until it becomes available or the operation is canceled.</remarks>
    ValueTask WaitForVariable(string variableName, int delay = 100, CancellationToken cancellationToken = default);
}
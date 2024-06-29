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
    Task LoadScript(string uri, string integrity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a style from the specified URI if it hasn't already been loaded  (through the ResourceLoader)
    /// </summary>
    /// <param name="uri">The URI of the style to load.</param>
    /// <param name="integrity">The integrity hash of the style for Subresource Integrity (SRI) validation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task will complete when the style is loaded or if it has already been loaded.</returns>
    Task LoadStyle(string uri, string integrity, CancellationToken cancellationToken = default);
}
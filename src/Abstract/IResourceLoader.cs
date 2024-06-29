using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Blazor.Utils.ResourceLoader.Abstract;

/// <summary>
/// A Blazor JavaScript module for dynamically loading scripts and styles
/// </summary>
public interface IResourceLoader
{
    /// <summary>
    /// Loads a script from the specified URI.
    /// </summary>
    /// <param name="uri">The URI of the script to load.</param>
    /// <param name="integrity">The integrity hash of the script for Subresource Integrity (SRI) validation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask LoadScript(string uri, string integrity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a style from the specified URI.
    /// </summary>
    /// <param name="uri">The URI of the style to load.</param>
    /// <param name="integrity">The integrity hash of the style for Subresource Integrity (SRI) validation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask LoadStyle(string uri, string integrity, CancellationToken cancellationToken = default);
}
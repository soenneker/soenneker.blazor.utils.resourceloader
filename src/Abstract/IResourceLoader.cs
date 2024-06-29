using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Blazor.Utils.ResourceLoader.Abstract;

/// <summary>
/// A Blazor JavaScript module for dynamically loading scripts and styles
/// </summary>
public interface IResourceLoader
{
    ValueTask LoadScript(string uri, string integrity, CancellationToken cancellationToken = default);

    ValueTask LoadStyle(string uri, string integrity, CancellationToken cancellationToken = default);
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;

namespace Soenneker.Blazor.Utils.ResourceLoader.Registrars;

/// <summary>
/// A Blazor JavaScript module for dynamically loading scripts and styles
/// </summary>
public static class ResourceLoaderRegistrar
{
    /// <summary>
    /// Adds <see cref="IResourceLoader"/> as a singleton service. <para/>
    /// </summary>
    public static void AddModuleImportUtil(this IServiceCollection services)
    {
        services.TryAddSingleton<IResourceLoader, ResourceLoader>();
    }
}
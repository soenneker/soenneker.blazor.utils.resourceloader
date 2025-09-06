using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.Utils.ModuleImport.Registrars;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;

namespace Soenneker.Blazor.Utils.ResourceLoader.Registrars;

/// <summary>
/// A Blazor JavaScript interop for dynamically loading scripts, styles, and modules
/// </summary>
public static class ResourceLoaderRegistrar
{
    /// <summary>
    /// Adds <see cref="IResourceLoader"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddResourceLoaderAsScoped(this IServiceCollection services)
    {
        services.AddModuleImportUtilAsScoped().TryAddScoped<IResourceLoader, ResourceLoader>();

        return services;
    }
}

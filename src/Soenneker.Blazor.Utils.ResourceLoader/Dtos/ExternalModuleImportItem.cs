using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Soenneker.Blazor.Utils.ResourceLoader.Dtos;

internal sealed class ExternalModuleImportItem : IAsyncDisposable
{
    public TaskCompletionSource<bool> ModuleLoadedTcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IJSObjectReference? ModuleReference { get; set; }

    public Task IsLoaded => ModuleLoadedTcs.Task;

    private bool _disposed;

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (ModuleReference != null)
            await ModuleReference.DisposeAsync();
    }
}

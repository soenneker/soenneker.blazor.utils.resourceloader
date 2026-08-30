using Soenneker.Atomics.ValueBools;
using System;
using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.ResourceLoader.Dtos;

internal sealed class ResourceLoadItem : IAsyncDisposable
{
    internal readonly TaskCompletionSource<bool> LoadedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task Loaded => LoadedTcs.Task;

    private ValueAtomicBool _disposed;

    public ValueTask DisposeAsync()
    {
        if (!_disposed.TrySetTrue())
            return ValueTask.CompletedTask;

        LoadedTcs.TrySetCanceled();

        return ValueTask.CompletedTask;
    }
}

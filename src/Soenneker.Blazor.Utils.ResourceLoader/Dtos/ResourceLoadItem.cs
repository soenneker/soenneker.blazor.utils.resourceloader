using Soenneker.Atomics.ValueBools;
using System;
using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.ResourceLoader.Dtos;

public sealed class ResourceLoadItem : IAsyncDisposable
{
    public readonly TaskCompletionSource<bool> LoadedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Loaded => LoadedTcs.Task;

    private ValueAtomicBool _disposed;

    public ValueTask DisposeAsync()
    {
        if (!_disposed.TrySetTrue())
            return ValueTask.CompletedTask;

        LoadedTcs.TrySetCanceled();

        return ValueTask.CompletedTask;
    }
}
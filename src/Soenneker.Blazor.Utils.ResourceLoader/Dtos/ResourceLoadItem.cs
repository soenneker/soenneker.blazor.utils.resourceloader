using Soenneker.Atomics.ValueBools;
using System;
using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.ResourceLoader.Dtos;

/// <summary>
/// Represents the resource load item.
/// </summary>
public sealed class ResourceLoadItem : IAsyncDisposable
{
    /// <summary>
    /// The loaded tcs.
    /// </summary>
    public readonly TaskCompletionSource<bool> LoadedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Gets or sets loaded.
    /// </summary>
    public Task Loaded => LoadedTcs.Task;

    private ValueAtomicBool _disposed;

    /// <summary>
    /// Asynchronously releases resources used by the current instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public ValueTask DisposeAsync()
    {
        if (!_disposed.TrySetTrue())
            return ValueTask.CompletedTask;

        LoadedTcs.TrySetCanceled();

        return ValueTask.CompletedTask;
    }
}
using Soenneker.Asyncs.Locks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.ResourceLoader;

internal sealed class ResourceCache<TArgs>(Func<TArgs, CancellationToken, ValueTask> load) : IAsyncDisposable where TArgs : notnull
{
    private readonly ConcurrentDictionary<TArgs, Task> _entries = new(1, 4);
    private readonly AsyncLock _gate = new();
    private bool _disposed;

    internal bool IsLoaded(TArgs args)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed), this);
        return _entries.TryGetValue(args, out Task? entry) && entry.IsCompletedSuccessfully;
    }

    internal async ValueTask Get(TArgs args, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TaskCompletionSource? completion = null;
        Task entry;
        bool created = false;
        using (await _gate.Lock(cancellationToken).ConfigureAwait(false))
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_entries.TryGetValue(args, out entry!))
            {
                completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                entry = completion.Task;
                _entries[args] = entry;
                created = true;
            }
        }
        if (created)
            _ = Initialize(args, completion!, cancellationToken);
        await entry.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task Initialize(TArgs args, TaskCompletionSource entry, CancellationToken cancellationToken)
    {
        try
        {
            await load(args, cancellationToken).ConfigureAwait(false);
            _entries.TryUpdate(args, Task.CompletedTask, entry.Task);
            entry.SetResult();
        }
        catch (Exception exception)
        {
            _entries.TryRemove(new KeyValuePair<TArgs, Task>(args, entry.Task));
            if (exception is OperationCanceledException cancelled)
                entry.SetCanceled(cancelled.CancellationToken);
            else
            {
                entry.SetException(exception);
                _ = entry.Task.Exception;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        ICollection<Task> entries;
        using (await _gate.Lock().ConfigureAwait(false))
        {
            if (_disposed)
                return;
            _disposed = true;
            entries = _entries.Values;
            _entries.Clear();
        }
        foreach (Task entry in entries)
        {
            // Failed loads have no .NET reference to release. The initiating call
            // receives the error; disposal only drains outstanding factories.
            try { await entry.ConfigureAwait(false); }
            catch { }
        }
    }
}

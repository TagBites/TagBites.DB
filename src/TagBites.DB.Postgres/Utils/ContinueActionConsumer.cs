#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace TagBites.Utils;

public class ContinueActionConsumer
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private CancellationTokenSource? _tokenSource;
    private readonly bool _allowCancellation;
    private readonly Func<CancellationToken, Task> _action;
    private int _pending;

    public bool IsRunning { get; private set; }

    public ContinueActionConsumer(Func<Task> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        _action = _ => action();
    }
    public ContinueActionConsumer(Func<CancellationToken, Task> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _allowCancellation = true;
    }


    public async Task ProcessAsync()
    {
        if (Interlocked.Exchange(ref _pending, 1) == 1)
            return;

        _tokenSource?.Cancel();

        await _semaphore.WaitAsync();
        IsRunning = true;

        try
        {
            while (Interlocked.Exchange(ref _pending, 0) == 1)
            {
                if (_allowCancellation && _tokenSource is not { IsCancellationRequested: false })
                    _tokenSource = new CancellationTokenSource();

                try
                {
                    await _action(_tokenSource?.Token ?? CancellationToken.None);
                }
                catch (OperationCanceledException)
                { }
            }
        }
        finally
        {
            IsRunning = false;
            _semaphore.Release(1);
        }
    }
    public void Cancel() => _pending = 0;
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SuperMediaSuites.Models;

namespace SuperMediaSuites.Services;

/// <summary>
/// Shared batch engine used by every converter tab. Runs a list of work items with
/// bounded concurrency, reports aggregate progress, supports cancellation, and raises
/// per-item <see cref="ItemCompleted"/>/<see cref="ItemFailed"/> events. Each tab supplies
/// a <c>Func&lt;TWork, CancellationToken, Task&gt;</c> that performs one conversion.
/// </summary>
public class BatchProcessor<TWork>
{
    /// <summary>Maximum number of jobs running at once (default 3).</summary>
    public int MaxDegreeOfParallelism { get; set; } = 3;

    /// <summary>Raised on the background thread when a single item finishes successfully.</summary>
    public event EventHandler<BatchItemEventArgs<TWork>>? ItemCompleted;

    /// <summary>Raised on the background thread when a single item throws. The exception is swallowed by the engine.</summary>
    public event EventHandler<BatchItemEventArgs<TWork>>? ItemFailed;

    public async Task RunAsync(
        IReadOnlyList<TWork> items,
        Func<TWork, CancellationToken, Task> workItem,
        Action<int, int>? aggregateProgress,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0) return;

        int completedCount = 0;
        int totalCount = items.Count;

        using var semaphore = new SemaphoreSlim(Math.Max(1, MaxDegreeOfParallelism));

        var tasks = new List<Task>(totalCount);
        foreach (var item in items)
        {
            tasks.Add(ProcessOneAsync(item, workItem, semaphore, () =>
            {
                var done = Interlocked.Increment(ref completedCount);
                aggregateProgress?.Invoke(done, totalCount);
            }, cancellationToken));
        }

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected on cancel; propagate so callers can finalise.
            throw;
        }
    }

    private async Task ProcessOneAsync(
        TWork item,
        Func<TWork, CancellationToken, Task> workItem,
        SemaphoreSlim semaphore,
        Action onDone,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await workItem(item, cancellationToken).ConfigureAwait(false);
            ItemCompleted?.Invoke(this, new BatchItemEventArgs<TWork>(item, null));
        }
        catch (OperationCanceledException)
        {
            // Count as done so the aggregate bar still completes on cancel.
            onDone();
            throw;
        }
        catch (Exception ex)
        {
            ItemFailed?.Invoke(this, new BatchItemEventArgs<TWork>(item, ex));
        }
        finally
        {
            semaphore.Release();
        }

        onDone();
    }
}

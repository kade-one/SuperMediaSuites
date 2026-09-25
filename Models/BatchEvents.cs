namespace SuperMediaSuites.Models;

/// <summary>Carried by <see cref="Services.BatchProcessor{TWork}"/> per-item events.</summary>
public sealed class BatchItemEventArgs<TWork> : EventArgs
{
    public TWork Item { get; }
    public Exception? Error { get; }
    public BatchItemEventArgs(TWork item, Exception? error)
    {
        Item = item;
        Error = error;
    }
}

/// <summary>Aggregate progress across a whole batch.</summary>
public readonly record struct BatchProgress(int Completed, int Total)
{
    public int Percentage => Total <= 0 ? 0 : (int)Math.Round((double)Completed / Total * 100);
}

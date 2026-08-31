using System.Collections.Concurrent;

namespace Hsp.ReaSharp;

/// <summary>
/// A SynchronizationContext that marshals continuations back to REAPER's main thread
/// via a registered timer callback. Set this as the current context on the main thread
/// during plugin initialization, then register a timer that calls ProcessQueue().
/// </summary>
public sealed class ReaperSynchronizationContext : SynchronizationContext
{
  private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _queue = new();
  private readonly int _mainThreadId = Environment.CurrentManagedThreadId;

  /// <summary>
  /// Drains the pending callback queue. Must be called on the main thread (e.g. from a REAPER timer).
  /// </summary>
  public void ProcessQueue()
  {
    while (_queue.TryDequeue(out var item))
      item.Callback(item.State);
  }

  /// <summary>
  /// Schedules a callback to run on the main thread asynchronously.
  /// Used by the runtime to dispatch async continuations.
  /// </summary>
  public override void Post(SendOrPostCallback d, object? state)
    => _queue.Enqueue((d, state));

  /// <summary>
  /// Runs a callback synchronously on the main thread. If already on the main thread,
  /// executes inline; otherwise blocks the calling thread until the main thread processes it.
  /// </summary>
  public override void Send(SendOrPostCallback d, object? state)
  {
    if (Environment.CurrentManagedThreadId == _mainThreadId)
    {
      d(state);
    }
    else
    {
      using var done = new ManualResetEventSlim(false);
      _queue.Enqueue((s =>
      {
        d(s);
        done.Set();
      }, state));
      done.Wait();
    }
  }
}

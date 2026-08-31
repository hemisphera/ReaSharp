using Hsp.ReaSharp.Models;
using Microsoft.Extensions.Logging;

namespace Hsp.ReaSharp.Utils;

public sealed class TrackMediaItemWatcher : IDisposable
{
  private readonly ILogger? _logger;
  private CancellationTokenSource? _cts;
  public Track Track { get; }
  private List<nint>? _lastSnapshot;

  public event EventHandler<MediaItem>? ItemAdded;


  public TrackMediaItemWatcher(Track track, ILogger? logger = null)
  {
    _logger = logger;
    Track = track;
  }


  public void Start()
  {
    Stop();
    var cts = new CancellationTokenSource();
    _cts = cts;
    var token = cts.Token;
    Task.Run(async () =>
    {
      _logger?.LogDebug("Starting watcher on {track}", Track.Name);
      _lastSnapshot = null;
      try
      {
        while (!token.IsCancellationRequested)
        {
          var count = Reaper.CountTrackMediaItems.Invoke(Track.ReaperHandle);
          var currentItems = Enumerable.Range(0, count).Select(i => Reaper.GetTrackMediaItem.Invoke(Track.ReaperHandle, i)).ToList();
          _lastSnapshot ??= currentItems;
          var removedItems = _lastSnapshot.Except(currentItems).ToList();
          var addedItems = currentItems.Except(_lastSnapshot).ToList();
          _lastSnapshot = currentItems;

          foreach (var removedItem in removedItems)
          {
            _logger?.LogDebug("Removed item {item} on track {track}", removedItem, Track.Name);
          }

          foreach (var addedItem in addedItems)
          {
            ItemAdded?.Invoke(this, MediaItem.FromHandle(addedItem));
            _logger?.LogDebug("Added item {addedItem} on track {track} ({count})", addedItem, Track.Name, addedItems.Count);
          }

          await Task.Delay(1, token);
        }
      }
      finally
      {
        _logger?.LogDebug("Watcher stopped");
      }
    }, token);
  }

  public void Stop()
  {
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = null;
    _lastSnapshot?.Clear();
    _lastSnapshot = null;
  }

  public void Restart()
  {
    Stop();
    Start();
  }

  public void Dispose()
  {
    Stop();
  }
}
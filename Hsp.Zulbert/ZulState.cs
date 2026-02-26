using System.Diagnostics;
using ReaSharp;
using ReaSharp.Models;

namespace Hsp.Zulbert;

public class ZulState
{
  public required Region CurrentRegion { get; init; }

  public required Transport Transport { get; init; }

  public required Track[] Tracks { get; init; }


  public static async Task<ZulState?> Create()
  {
    var transport = new Transport();
    var region = Region.Enumerate().FirstOrDefault(r => r.IsActive(transport.PlayheadOrCursorPosition));
    if (region == null) return null;
    ReaperLogger.LogInformation($"Found active region: {region})");

    var tracks = Track.Enumerate().ToList();
    var containerTrack = tracks.FirstOrDefault(t => t.Name.Equals("Songs", StringComparison.OrdinalIgnoreCase));
    if (containerTrack == null) return null;
    ReaperLogger.LogInformation($"Found container track: {containerTrack})");

    var songTracks = tracks.Where(i => i.Index > containerTrack.Index);
    var tree = TrackTreeItem.Build(songTracks);
    var currentSong = tree.FirstOrDefault(ti => ti.Track.Name.Equals(region.Name, StringComparison.OrdinalIgnoreCase));
    if (currentSong == null) return null;
    ReaperLogger.LogInformation($"Found song track: {currentSong.Track})");

    ReaperLogger.LogInformation($"Toggling {tree.Length}");
    foreach (var item in tree)
    {
      ToggleSong(item, item.Track == currentSong.Track);
    }

    return new ZulState
    {
      CurrentRegion = region,
      Transport = transport,
      Tracks = currentSong.Children.Select(c => c.Track).ToArray()
    };
  }

  private static void ToggleSong(TrackTreeItem item, bool isEnabled)
  {
    ReaperLogger.LogDebug("Toggling " + item.Track + " to " + (isEnabled ? "enabled" : "disabled"));
    item.Track.Mute = !isEnabled;
    item.Track.FxBypassed = !isEnabled;
    item.Track.ShowInTcp = isEnabled;
    foreach (var child in item.Children)
    {
      ToggleSong(child, isEnabled);
    }
  }


  private ZulState()
  {
  }

  public async Task Run()
  {
    if (!WaitForStart()) return;
    while (true)
    {
      await Tick();
      if (!Transport.IsPlaying) break;
    }
  }

  private async Task Tick()
  {
  }

  private async Task BuildState()
  {
  }

  private bool WaitForStart()
  {
    var sw = Stopwatch.StartNew();
    while (true)
    {
      Transport.Update();
      if (Transport.IsPlaying) return true;
      if (sw.Elapsed.TotalSeconds > 3) return false;
    }
  }
}
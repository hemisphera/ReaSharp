namespace ReaSharp.Models;

public class TrackMediaItem
{
  public int Index { get; }

  private readonly Track _track;

  public IntPtr ReaperHandle { get; }

  internal TrackMediaItem(Track track, int i)
  {
    Index = i;
    _track = track;
    ReaperHandle = Reaper.GetTrackMediaItem(_track.ReaperHandle, i);
  }
}
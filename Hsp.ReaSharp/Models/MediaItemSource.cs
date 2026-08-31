namespace Hsp.ReaSharp.Models;

public class MediaItemSource : ReaperObject
{
  public override nint ReaperHandle { get; }

  public MediaItemSource(nint getMediaItemTakeSource)
  {
    ReaperHandle = getMediaItemTakeSource;
  }
}
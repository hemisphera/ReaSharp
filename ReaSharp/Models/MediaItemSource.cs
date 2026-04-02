namespace ReaSharp.Models;

public class MediaItemSource : ReaperObject
{
  public override IntPtr ReaperHandle { get; }

  public MediaItemSource(nint getMediaItemTakeSource)
  {
    ReaperHandle = getMediaItemTakeSource;
  }
}
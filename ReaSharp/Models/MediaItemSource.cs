namespace ReaSharp.Models;

public class MediaItemSource
{
  public IntPtr ReaperHandle { get; }

  public MediaItemSource(nint getMediaItemTakeSource)
  {
    ReaperHandle = getMediaItemTakeSource;
  }
}
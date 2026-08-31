using System.Runtime.InteropServices;
using Hsp.ReaSharp.RppXml;

namespace Hsp.ReaSharp.Models;

public sealed class MediaItem : IArrangeItem
{
  public static MediaItem FromHandle(IntPtr handle)
  {
    return new MediaItem(handle);
  }

  public static MediaItem FromByTrackIndex(Track track, int index)
  {
    return FromHandle(Reaper.GetTrackMediaItem.Invoke(track.ReaperHandle, index));
  }


  public Track Track => Track.FromHandle(Reaper.GetMediaItemTrack.Invoke(ReaperHandle));

  public bool Selected
  {
    get => GetValue("B_UISEL") != 0;
    set => SetValue("B_UISEL", value ? 1 : 0);
  }

  public TimeSpan Length
  {
    get => TimeSpan.FromSeconds(Math.Round(GetValue("D_LENGTH"), 5));
    set => SetValue("D_LENGTH", value.TotalSeconds);
  }

  public TimeSpan Start
  {
    get => TimeSpan.FromSeconds(Math.Round(GetValue("D_POSITION"), 5));
    set => SetValue("D_POSITION", value.TotalSeconds);
  }

  public TimeSpan End => Start + Length;

  public bool LoopSource
  {
    get => GetValue("B_LOOPSRC") != 0;
    set => SetValue("B_LOOPSRC", value ? 1 : 0);
  }

  public IntPtr ReaperHandle { get; }


  private MediaItem(IntPtr handle)
  {
    ReaperHandle = handle;
  }

  private double GetValue(string paramName)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    try
    {
      var value = Reaper.GetMediaItemInfo_Value.Invoke(ReaperHandle, ptr);
      return value;
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }
  }

  private void SetValue(string paramName, double newValue)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    try
    {
      if (!Reaper.SetMediaItemInfo_Value.Invoke(ReaperHandle, ptr, newValue))
      {
        throw new Exception($"Unable to set value '{newValue}' for '{paramName}'.");
      }
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }
  }


  public MediaItemTake CreateTake()
  {
    var handle = Reaper.AddTakeToMediaItem.Invoke(ReaperHandle);
    return MediaItemTake.FromHandle(handle);
  }

  public MediaItemTake? GetActiveTake()
  {
    var handle = Reaper.GetActiveTake.Invoke(ReaperHandle);
    return handle != IntPtr.Zero ? MediaItemTake.FromHandle(handle) : null;
  }

  public RppNode? GetStateChunk()
  {
    var value = Marshal.AllocHGlobal(Reaper.NeedBigBufferSize);
    try
    {
      var content = Reaper.GetItemStateChunk.Invoke(ReaperHandle, value, Reaper.NeedBigBufferSize, false)
        ? Marshal.PtrToStringAnsi(value)
        : null;
      return string.IsNullOrEmpty(content) ? null : RppReader.Read(content);
    }
    finally
    {
      Marshal.FreeHGlobal(value);
    }
  }

  public List<MediaItemTake> EnumerateTakes()
  {
    var result = new List<MediaItemTake>();
    var count = Reaper.GetMediaItemNumTakes.Invoke(ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      var handle = Reaper.GetMediaItemTake.Invoke(ReaperHandle, i);
      result.Add(MediaItemTake.FromHandle(handle));
    }

    return result;
  }

  public void SelectExclusive()
  {
    var selectedItems = Track.Project.EnumerateMediaItems().Where(i => i.Selected).ToList();
    foreach (var item in selectedItems)
    {
      item.Selected = false;
    }

    Selected = true;
  }

  public void Delete()
  {
    Reaper.DeleteTrackMediaItem.Invoke(Track.ReaperHandle, ReaperHandle);
  }

  public override string ToString()
  {
    return $"{ReaperHandle} ({Start} - {Start + Length})";
  }
}
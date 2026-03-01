using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public sealed class TrackMediaItem
{
  public static TrackMediaItem FromHandle(IntPtr handle)
  {
    return new TrackMediaItem(handle);
  }

  public static TrackMediaItem FromByTrackIndex(Track track, int index)
  {
    return FromHandle(Reaper.GetTrackMediaItem(track.ReaperHandle, index));
  }

  public static List<TrackMediaItem> Enumerate(Track track)
  {
    var result = new List<TrackMediaItem>();
    var count = Reaper.CountTrackMediaItems(track.ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      var handle = Reaper.GetTrackMediaItem(track.ReaperHandle, i);
      result.Add(FromHandle(handle));
    }

    return result;
  }

  public static List<TrackMediaItem> Enumerate(Project? project = null)
  {
    project ??= Project.Default;
    var result = new List<TrackMediaItem>();
    var count = Reaper.CountMediaItems(project.ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      var handle = Reaper.GetMediaItem(project.ReaperHandle, i);
      result.Add(FromHandle(handle));
    }

    return result;
  }


  public Track Track => Track.FromHandle(Reaper.GetMediaItemTrack(ReaperHandle));

  public bool Selected
  {
    get => GetValue("I_SELECTED") != 0;
    set => SetValue("I_SELECTED", value ? 1 : 0);
  }

  public TimeSpan Length
  {
    get => TimeSpan.FromSeconds(GetValue("D_LENGTH"));
    set => SetValue("D_LENGTH", value.TotalSeconds);
  }

  public TimeSpan Position
  {
    get => TimeSpan.FromSeconds(GetValue("D_POSITION"));
    set => SetValue("D_POSITION", value.TotalSeconds);
  }

  public IntPtr ReaperHandle { get; }


  private TrackMediaItem(IntPtr handle)
  {
    ReaperHandle = handle;
  }

  private double GetValue(string paramName)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    try
    {
      var value = Reaper.GetMediaItemInfo_Value(ReaperHandle, ptr);
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
      if (!Reaper.SetMediaItemInfo_Value(ReaperHandle, ptr, newValue))
      {
        throw new Exception($"Unable to set value '{newValue}' for '{paramName}'.");
      }
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }
  }


  public TrackMediItemTake CreateTake()
  {
    var handle = Reaper.AddTakeToMediaItem(ReaperHandle);
    return TrackMediItemTake.FromHandle(handle);
  }

  public List<TrackMediItemTake> EnumerateTakes()
  {
    var result = new List<TrackMediItemTake>();
    var count = Reaper.GetMediaItemNumTakes(ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      var handle = Reaper.GetMediaItemTake(ReaperHandle, i);
      result.Add(TrackMediItemTake.FromHandle(handle));
    }

    return result;
  }

  public void SelectExclusive()
  {
    var selectedItems = Enumerate(Track.Project).Where(i => i.Selected).ToList();
    foreach (var item in selectedItems)
    {
      item.Selected = false;
    }

    Selected = true;
  }
}
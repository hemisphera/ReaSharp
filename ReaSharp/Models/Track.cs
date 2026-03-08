using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public sealed class Track
{
  public static IEnumerable<Track> Enumerate(Project? project = null)
  {
    project ??= Project.Default;
    List<Track> tracks = [];
    for (var i = 0; i < Reaper.GetNumTracks(); i++)
    {
      tracks.Add(FromIndex(i, project));
    }

    return tracks;
  }

  public static Track FromIndex(int index, Project? project = null)
  {
    project ??= Project.Default;
    var handle = Reaper.GetTrack(project.ReaperHandle, index);
    return FromHandle(handle);
  }

  public static Track FromHandle(IntPtr handle)
  {
    return new Track(handle);
  }


  public IntPtr ReaperHandle { get; }
  public Project Project => Project.FromHandle((IntPtr)GetValue("P_PROJECT"));
  public int Index { get; }
  public Guid Id { get; }

  public string Name
  {
    get => GetStringValue("P_NAME") ?? string.Empty;
    set => SetStringValue("P_NAME", value);
  }

  public bool Mute
  {
    get => (int)GetValue("B_MUTE") != 0;
    set => SetValue("B_MUTE", value ? 1.0 : 0.0);
  }

  public TrackSoloState Solo
  {
    get => (TrackSoloState)(int)GetValue("I_SOLO");
    set => SetValue("B_MUTE", (double)value);
  }

  public bool FxBypassed
  {
    get => (int)GetValue("I_FXEN") == 0;
    set => SetValue("I_FXEN", value ? 0 : 1);
  }

  public int FolderLevel
  {
    get => (int)GetValue("I_FOLDERDEPTH");
    set => SetValue("I_FOLDERDEPTH", value);
  }

  public bool ShowInMixer
  {
    get => (int)GetValue("B_SHOWINMIXER") == 0;
    set => SetValue("B_SHOWINMIXER", value ? 1 : 0);
  }

  public bool ShowInTcp
  {
    get => (int)GetValue("B_SHOWINTCP") == 0;
    set => SetValue("B_SHOWINTCP", value ? 1 : 0);
  }


  private Track(IntPtr trackHandle)
  {
    ReaperHandle = trackHandle;
    Index = (int)GetValue("IP_TRACKNUMBER");
    Id = Guid.TryParse(GetStringValue("GUID"), out var guid) ? guid : Guid.Empty;
  }


  private string? GetStringValue(string paramName)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    var value = Marshal.AllocHGlobal(Reaper.NeedBigBufferSize);
    try
    {
      return Reaper.GetSetMediaTrackInfo_String(ReaperHandle, ptr, value, false)
        ? Marshal.PtrToStringAnsi(value)
        : null;
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
      Marshal.FreeHGlobal(value);
    }
  }

  private void SetStringValue(string paramName, string? value)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    var ptrValue = Marshal.StringToHGlobalAnsi(value);
    try
    {
      Reaper.GetSetMediaTrackInfo_String(ReaperHandle, ptr, ptrValue, true);
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
      Marshal.FreeHGlobal(ptrValue);
    }
  }

  private double GetValue(string paramName)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    try
    {
      var value = Reaper.GetMediaTrackInfo_Value(ReaperHandle, ptr);
      //ReaperLogger.LogDebug($"Read '{paramName}': '{value}'.");
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
      //ReaperLogger.LogDebug($"Setting '{paramName}' to '{newValue}'.");
      if (!Reaper.SetMediaTrackInfo_Value(ReaperHandle, ptr, newValue))
      {
        throw new Exception($"Unable to set value '{newValue}' for '{paramName}'.");
      }
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }
  }


  public IEnumerable<TrackMediaItem> EnumerateMediaItems()
  {
    var track = Reaper.GetTrack(Project.ReaperHandle, Index);
    var itemCount = Reaper.CountTrackMediaItems(track);
    return Enumerable.Range(0, itemCount).Select(i => TrackMediaItem.FromByTrackIndex(this, i));
  }

  public TrackMediaItem CreateEmptyItem(TimeSpan? position = null, TimeSpan? length = null)
  {
    var handle = Reaper.AddMediaItemToTrack(ReaperHandle);
    //ReaperLogger.LogDebug($"Created media item {handle}");
    var item = TrackMediaItem.FromHandle(handle);
    item.Position = position ?? TimeSpan.FromSeconds(0);
    item.Length = length ?? TimeSpan.FromSeconds(1);
    return item;
  }

  public TrackMediaItem CreateMidiItem(TimeSpan? position = null, TimeSpan? length = null)
  {
    length ??= TimeSpan.FromSeconds(1);
    position ??= TimeSpan.FromSeconds(0);
    var handle = Reaper.CreateNewMIDIItemInProj(
      ReaperHandle,
      position.Value.TotalSeconds, (position + length).Value.TotalSeconds,
      IntPtr.Zero);
    return TrackMediaItem.FromHandle(handle);
  }

  public override string ToString()
  {
    return $"{Id}: {Name}";
  }
}
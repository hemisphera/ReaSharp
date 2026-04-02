using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public class Project : ReaperObject
{
  public static readonly Project Default = new(IntPtr.Zero);
  public static Project Current => GetProjectByIndex(-1) ?? throw new Exception("Unable to get current project");
  public static Project? CurrentlyRendering => GetProjectByIndex(0x40000000);

  public static Project? GetProjectByIndex(int index)
  {
    const int bufSize = 4096;
    var filename = Marshal.AllocHGlobal(bufSize);
    try
    {
      var prj = Reaper.EnumProjects(index, filename, bufSize);
      if (prj == IntPtr.Zero) return null;
      return new Project(prj)
      {
        Index = index,
        Filename = Marshal.PtrToStringAnsi(filename)
      };
    }
    finally
    {
      Marshal.FreeHGlobal(filename);
    }
  }

  public static Project FromHandle(IntPtr handle)
  {
    return new Project(handle);
  }

  public static IEnumerable<Project> Enumerate()
  {
    List<Project> projects = [];
    var ci = -1;
    while (true)
    {
      ci++;
      var prj = GetProjectByIndex(ci);
      if (prj == null) break;
      projects.Add(prj);
    }

    return projects;
  }


  public override IntPtr ReaperHandle { get; }

  public int Index { get; init; } = -1;

  public string? Filename { get; init; }


  private Project(IntPtr handle)
  {
    ReaperHandle = handle;
  }

  public List<Track> GetTracks()
  {
    return Track.Enumerate(this).ToList();
  }

  public List<Track> GetSelectedTracks()
  {
    var tracks = new List<Track>();
    var count = Reaper.CountSelectedTracks2(ReaperHandle, false);
    for (var i = 0; i < count; i++)
    {
      var trackHandle = Reaper.GetSelectedTrack2(ReaperHandle, i, false);
      tracks.Add(Track.FromHandle(trackHandle));
    }

    return tracks;
  }


  public int InvokeCommand(int id)
  {
    Reaper.Main_OnCommandEx(id, 0, ReaperHandle);
    return id;
  }

  public int InvokeCommand(string id)
  {
    var handle = Marshal.StringToHGlobalAnsi(id);
    var resp = Reaper.NamedCommandLookup(handle);
    Marshal.FreeHGlobal(handle);
    return resp > 0 ? InvokeCommand(resp) : -1;
  }

  public void ClearSelection()
  {
    SetSelection(TimeSpan.Zero, TimeSpan.Zero);
  }

  public void SetSelection(TimeSpan start, TimeSpan end)
  {
    unsafe
    {
      var startVal = start.TotalSeconds;
      var endVal = end.TotalSeconds;
      Reaper.GetSet_LoopTimeRange2(ReaperHandle, true, false, (IntPtr)(&startVal), (IntPtr)(&endVal), false);
    }
  }

  public void SetSelection(IArrangeItem ai)
  {
    SetSelection(ai.Start, ai.End);
  }

  public List<TrackMediaItem> GetSelectedItems(int? maxCount = null)
  {
    var items = TrackMediaItem.Enumerate(this);
    List<TrackMediaItem> result = [];
    foreach (var item in items)
    {
      if (item.Selected)
      {
        result.Add(item);
      }

      if (maxCount.HasValue && result.Count >= maxCount.Value) break;
    }

    return result;
  }

  public void StartStopRecordingAtNextMeasure()
  {
    Reaper.Main_OnCommandEx(40003, 0, ReaperHandle);
  }

  public void ZoomToTimeSelection()
  {
    Reaper.Main_OnCommandEx(40031, 0, ReaperHandle);
  }

  public void BeginUndoBlock()
  {
    Reaper.Undo_BeginBlock2(ReaperHandle);
  }

  public void EndUndoBlock(string description)
  {
    var str = Marshal.StringToHGlobalAnsi(description);
    Reaper.Undo_EndBlock2(ReaperHandle, str, -1);
    Marshal.FreeHGlobal(str);
  }

  public void ZoomTo(IArrangeItem item)
  {
    SetSelection(item);
    ZoomToTimeSelection();
    ClearSelection();
  }

  public override string ToString()
  {
    return $"{Index}: {Filename}";
  }
}
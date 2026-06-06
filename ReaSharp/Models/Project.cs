using System.Runtime.InteropServices;
using ReaSharp.RppXml;

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
      var prj = Reaper.EnumProjects.Invoke(index, filename, bufSize);
      if (prj == nint.Zero) return null;
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

  public string? RecordingPath
  {
    get
    {
      const int bufSize = 4096;
      var path = Marshal.AllocHGlobal(bufSize);
      try
      {
        Reaper.GetProjectPathEx.Invoke(ReaperHandle, path, bufSize);
        return Marshal.PtrToStringAnsi(path);
      }
      finally
      {
        Marshal.FreeHGlobal(path);
      }
    }
  }

  public string? Notes
  {
    get
    {
      const int bufferSize = 4 * 4096;
      var buffer = Marshal.AllocHGlobal(bufferSize);
      try
      {
        Reaper.GetSetProjectNotes.Invoke(ReaperHandle, false, buffer, bufferSize);
        return Marshal.PtrToStringAnsi(buffer);
      }
      finally
      {
        Marshal.FreeHGlobal(buffer);
      }
    }
    set
    {
      const int bufferSize = 4 * 4096;
      var buffer = Marshal.StringToHGlobalAnsi(value);
      try
      {
        Reaper.GetSetProjectNotes.Invoke(ReaperHandle, true, buffer, bufferSize);
      }
      finally
      {
        Marshal.FreeHGlobal(buffer);
      }
    }
  }

  private Project(IntPtr handle)
  {
    ReaperHandle = handle;
  }

  public List<Track> GetTracks()
  {
    return Track.Enumerate(this).ToList();
  }

  public Track? GetSelectedTrack()
  {
    return GetSelectedTracks().FirstOrDefault();
  }

  public List<Track> GetSelectedTracks()
  {
    var tracks = new List<Track>();
    var count = Reaper.CountSelectedTracks2.Invoke(ReaperHandle, false);
    for (var i = 0; i < count; i++)
    {
      var trackHandle = Reaper.GetSelectedTrack2.Invoke(ReaperHandle, i, false);
      tracks.Add(Track.FromHandle(trackHandle));
    }

    return tracks;
  }


  public int InvokeCommand(int id)
  {
    Reaper.Main_OnCommandEx.Invoke(id, 0, ReaperHandle);
    return id;
  }

  public int InvokeCommand(string id)
  {
    var handle = Marshal.StringToHGlobalAnsi(id);
    var resp = Reaper.NamedCommandLookup.Invoke(handle);
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
      Reaper.GetSet_LoopTimeRange2.Invoke(ReaperHandle, true, false, (IntPtr)(&startVal), (IntPtr)(&endVal), false);
    }
  }

  public void SetSelection(IArrangeItem ai)
  {
    SetSelection(ai.Start, ai.End);
  }


  public List<TrackMediaItem> EnumerateMediaItems()
  {
    var result = new List<TrackMediaItem>();
    var count = Reaper.CountMediaItems.Invoke(ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      var handle = Reaper.GetMediaItem.Invoke(ReaperHandle, i);
      result.Add(TrackMediaItem.FromHandle(handle));
    }

    return result;
  }

  public List<TrackMediaItem> GetSelectedMediaItems(int? maxCount = null)
  {
    var items = EnumerateMediaItems();
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
    Reaper.Main_OnCommandEx.Invoke(40003, 0, ReaperHandle);
  }

  public void ZoomToTimeSelection()
  {
    Reaper.Main_OnCommandEx.Invoke(40031, 0, ReaperHandle);
  }

  public void BeginUndoBlock()
  {
    Reaper.Undo_BeginBlock2.Invoke(ReaperHandle);
  }

  public void EndUndoBlock(string description)
  {
    var str = Marshal.StringToHGlobalAnsi(description);
    Reaper.Undo_EndBlock2.Invoke(ReaperHandle, str, -1);
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

  public Track? GetTrackByNumber(int number)
  {
    return GetTracks().FirstOrDefault(t => t.Number == number);
  }

  public Track? GetTrackByIndex(int index)
  {
    return GetTracks().FirstOrDefault(t => t.Index == index);
  }

  public void InsertMedia(byte[] contents, string name)
  {
    var recordingPath = RecordingPath ?? throw new InvalidOperationException("Recording path is not set.");
    var filePath = Path.Combine(recordingPath, name);
    File.WriteAllBytes(filePath, contents);
    InsertMedia(filePath);
  }

  public void InsertMedia(string path)
  {
    var str = Marshal.StringToHGlobalAnsi(path);
    try
    {
      Reaper.InsertMedia.Invoke(str, 0);
    }
    finally
    {
      Marshal.FreeHGlobal(str);
    }
  }
}
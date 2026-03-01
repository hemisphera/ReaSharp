using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public class Project
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


  public IntPtr ReaperHandle { get; }

  public int Index { get; init; } = -1;

  public string? Filename { get; init; }


  private Project(IntPtr handle)
  {
    ReaperHandle = handle;
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


  public override string ToString()
  {
    return $"{Index}: {Filename}";
  }
}
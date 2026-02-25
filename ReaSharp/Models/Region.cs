using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public sealed class Region
{
  public required int Id { get; init; }
  public required string Name { get; init; }
  public TimeSpan Start { get; init; }
  public TimeSpan End { get; init; }
  public TimeSpan Duration => End - Start;

  public static IEnumerable<Region> Enumerate(Project? project = null)
  {
    project ??= Project.Default;
    var regions = new List<Region>();

    unsafe
    {
      var idx = 0;
      while (true)
      {
        var isRegion = 0;
        var pos = 0.0;
        var end = 0.0;
        var namePtr = IntPtr.Zero;
        var markOrRegionIndex = 0;
        var color = 0;

        var result = Reaper.EnumProjectMarkers3(
          project.ReaperHandle,
          idx,
          (IntPtr)(&isRegion),
          (IntPtr)(&pos),
          (IntPtr)(&end),
          (IntPtr)(&namePtr),
          (IntPtr)(&markOrRegionIndex),
          (IntPtr)(&color));

        if (result == 0) break;

        if (isRegion != 0)
        {
          regions.Add(new Region
          {
            Id = markOrRegionIndex,
            Name = Marshal.PtrToStringAnsi(namePtr) ?? string.Empty,
            Start = TimeSpan.FromSeconds(pos),
            End = TimeSpan.FromSeconds(end)
          });
        }

        idx++;
      }
    }

    return regions;
  }

  public override string ToString()
  {
    return $"{Id}: {Name} [{Start} - {End}]";
  }
}
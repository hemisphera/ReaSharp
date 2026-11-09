using System.Runtime.InteropServices;

namespace Hsp.ReaSharp.Models;

public sealed class Marker
{
  public required int Id { get; init; }
  public required string Name { get; init; }
  public TimeSpan Start { get; init; }

  public static IEnumerable<Marker> Enumerate(Project? project = null)
  {
    project ??= Project.Default;
    var regions = new List<Marker>();

    unsafe
    {
      var idx = 0;
      while (true)
      {
        var isRegion = 0;
        var pos = 0.0;
        var end = 0.0;
        var namePtr = nint.Zero;
        var markOrRegionIndex = 0;
        var color = 0;

        var result = Reaper.EnumProjectMarkers3.Invoke(
          project.ReaperHandle,
          idx,
          (nint)(&isRegion),
          (nint)(&pos),
          (nint)(&end),
          (nint)(&namePtr),
          (nint)(&markOrRegionIndex),
          (nint)(&color));

        if (result == 0) break;

        if (isRegion == 0)
        {
          regions.Add(new Marker
          {
            Id = markOrRegionIndex,
            Name = Marshal.PtrToStringAnsi(namePtr) ?? string.Empty,
            Start = TimeSpan.FromSeconds(pos)
          });
        }

        idx++;
      }
    }

    return regions;
  }

  public override string ToString()
  {
    return $"{Id}: {Name} [{Start}]";
  }
}
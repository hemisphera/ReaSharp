using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public class FxInstance
{
  public ReaperObject Owner { get; }
  public Track? Track => Owner as Track;
  public TrackMediaItemTake? Take => Owner as TrackMediaItemTake;
  public FxInstanceType Type { get; }

  public string? Name => GetFxName();

  public int ParameterCount
  {
    get
    {
      if (Track != null)
        return Reaper.TrackFX_GetNumParams(Owner.ReaperHandle, Index);
      if (Take != null)
        return Reaper.TakeFX_GetNumParams(Owner.ReaperHandle, Index);
      return 0;
    }
  }

  public int Index { get; }


  public static List<FxInstance> EnumerateFromTrack(Track track)
  {
    var result = new List<FxInstance>();
    var count = Reaper.TrackFX_GetCount(track.ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      result.Add(new FxInstance(track, i));
    }

    return result;
  }

  public static FxInstance FromTrackByIndex(Track track, int idx)
  {
    return new FxInstance(track, idx);
  }

  public static List<FxInstance> EnumerateFromTake(TrackMediaItemTake take)
  {
    var result = new List<FxInstance>();
    var count = Reaper.TakeFX_GetCount(take.ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      result.Add(new FxInstance(take, i));
    }

    return result;
  }

  public static FxInstance FromTakeByIndex(TrackMediaItemTake take, int idx)
  {
    return new FxInstance(take, idx);
  }


  private FxInstance(ReaperObject owner, int index)
  {
    Owner = owner;
    Index = index;
    Type = owner is Track ? FxInstanceType.Track : FxInstanceType.Take;
  }


  public FxInstanceParameter GetParameter(int parameterIndex)
  {
    return FxInstanceParameter.FromTrackFx(this, parameterIndex);
  }

  private string? GetFxName()
  {
    const int bufferSize = 1024;
    var buffer = Marshal.AllocHGlobal(bufferSize);
    try
    {
      var ok = Track != null
        ? Reaper.TrackFX_GetFXName(Owner.ReaperHandle, Index, buffer, bufferSize)
        : Reaper.TakeFX_GetFXName(Owner.ReaperHandle, Index, buffer, bufferSize);
      return ok ? Marshal.PtrToStringAnsi(buffer) : null;
    }
    finally
    {
      Marshal.FreeHGlobal(buffer);
    }
  }
}
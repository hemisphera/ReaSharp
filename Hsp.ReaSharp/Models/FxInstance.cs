using System.Runtime.InteropServices;

namespace Hsp.ReaSharp.Models;

public class FxInstance
{
  public ReaperObject Owner { get; }
  public Track? Track => Owner as Track;
  public MediaItemTake? Take => Owner as MediaItemTake;
  public FxInstanceType Type { get; }

  public string? Name => GetFxName();

  public int ParameterCount
  {
    get
    {
      if (Track != null)
        return Reaper.TrackFX_GetNumParams.Invoke(Owner.ReaperHandle, Index);
      if (Take != null)
        return Reaper.TakeFX_GetNumParams.Invoke(Owner.ReaperHandle, Index);
      return 0;
    }
  }

  public int Index { get; }

  public bool Offline
  {
    get
    {
      if (Track != null)
        return Reaper.TrackFX_GetOffline.Invoke(Owner.ReaperHandle, Index);
      if (Take != null)
        return Reaper.TakeFX_GetOffline.Invoke(Owner.ReaperHandle, Index);
      return false;
    }
    set
    {
      if (Offline == value) return;
      if (Track != null)
        Reaper.TrackFX_SetOffline.Invoke(Owner.ReaperHandle, Index, value);
      if (Take != null)
        Reaper.TakeFX_SetOffline.Invoke(Owner.ReaperHandle, Index, value);
    }
  }

  public bool Bypass
  {
    get
    {
      if (Track != null)
        return Reaper.TrackFX_GetEnabled.Invoke(Owner.ReaperHandle, Index);
      if (Take != null)
        return Reaper.TakeFX_GetEnabled.Invoke(Owner.ReaperHandle, Index);
      return false;
    }
    set
    {
      if (Bypass == value) return;
      if (Track != null)
        Reaper.TrackFX_SetEnabled.Invoke(Owner.ReaperHandle, Index, value);
      if (Take != null)
        Reaper.TakeFX_SetEnabled.Invoke(Owner.ReaperHandle, Index, value);
    }
  }


  public static List<FxInstance> EnumerateFromTrack(Track track)
  {
    var result = new List<FxInstance>();
    var count = Reaper.TrackFX_GetCount.Invoke(track.ReaperHandle);
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

  public static List<FxInstance> EnumerateFromTake(MediaItemTake take)
  {
    var result = new List<FxInstance>();
    var count = Reaper.TakeFX_GetCount.Invoke(take.ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      result.Add(new FxInstance(take, i));
    }

    return result;
  }

  public static FxInstance FromTakeByIndex(MediaItemTake take, int idx)
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
        ? Reaper.TrackFX_GetFXName.Invoke(Owner.ReaperHandle, Index, buffer, bufferSize)
        : Reaper.TakeFX_GetFXName.Invoke(Owner.ReaperHandle, Index, buffer, bufferSize);
      return ok ? Marshal.PtrToStringAnsi(buffer) : null;
    }
    finally
    {
      Marshal.FreeHGlobal(buffer);
    }
  }
}
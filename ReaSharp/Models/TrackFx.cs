using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ReaSharp.Models;

public class TrackFx
{
  public Track Track { get; }

  public string? Name => GetFxName();

  public int ParameterCount => Reaper.TrackFX_GetNumParams(Track.ReaperHandle, Index);

  public int Index { get; }


  public static List<TrackFx> Enumerate(Track track)
  {
    var result = new List<TrackFx>();
    var count = Reaper.TrackFX_GetCount(track.ReaperHandle);
    for (var i = 0; i < count; i++)
    {
      result.Add(new TrackFx(track, i));
    }

    return result;
  }

  public static TrackFx FromIndex(Track track, int idx)
  {
    return new TrackFx(track, idx);
  }


  private TrackFx(Track track, int index)
  {
    Track = track;
    Index = index;
  }


  private string? GetFxName()
  {
    const int bufferSize = 1024;
    var buffer = Marshal.AllocHGlobal(bufferSize);
    try
    {
      return !Reaper.TrackFX_GetFXName(Track.ReaperHandle, Index, buffer, bufferSize)
        ? null
        : Marshal.PtrToStringAnsi(buffer);
    }
    finally
    {
      Marshal.FreeHGlobal(buffer);
    }
  }

  public string? GetNamedParameter(int paramIndex)
  {
    const int bufferSize = 1024;
    var buffer = Marshal.AllocHGlobal(bufferSize);
    try
    {
      return !Reaper.TrackFX_GetNamedConfigParm(Track.ReaperHandle, Index, paramIndex, buffer, bufferSize)
        ? null
        : Marshal.PtrToStringAnsi(buffer);
    }
    finally
    {
      Marshal.FreeHGlobal(buffer);
    }
  }

  public ParameterValue GetValue(int paramIndex)
  {
    double minVal = 0, maxVal = 0;
    unsafe
    {
      var value = Reaper.TrackFX_GetParam(Track.ReaperHandle, Index, paramIndex, (nint)(&minVal), (nint)(&maxVal));
      return new ParameterValue(minVal, maxVal, value);
    }
  }

  public TrackFxEnvelope? GetEnvelope(int paramIndex, bool allowCreate = false)
  {
    var ptr = Reaper.GetFXEnvelope(Track.ReaperHandle, Index, paramIndex, allowCreate);
    return ptr == IntPtr.Zero ? null : TrackFxEnvelope.FromHandle(ptr);
  }
}
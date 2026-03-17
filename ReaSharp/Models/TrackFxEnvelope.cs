using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public class TrackFxEnvelope
{
  public IntPtr ReaperHandle { get; }

  public Track Track { get; }

  public bool Visible
  {
    get => GetStringValue("VISIBLE") == "1";
    set => SetStringValue("VISIBLE", value ? "1" : "0");
  }

  public bool Active
  {
    get => GetStringValue("ACTIVE") == "1";
    set => SetStringValue("ACTIVE", value ? "1" : "0");
  }

  public bool Armed
  {
    get => GetStringValue("ARM") == "1";
    set => SetStringValue("ARM", value ? "1" : "0");
  }

  public string? Name
  {
    get
    {
      const int bufferSize = 1024;
      var value = Marshal.AllocHGlobal(bufferSize);
      try
      {
        return Reaper.GetEnvelopeName(ReaperHandle, value, bufferSize)
          ? Marshal.PtrToStringAnsi(value)
          : null;
      }
      finally
      {
        Marshal.FreeHGlobal(value);
      }
    }
  }

  public int FxIndex { get; }

  public int FxParameterIndex { get; }


  public static TrackFxEnvelope FromHandle(IntPtr handle)
  {
    return new TrackFxEnvelope(handle);
  }


  private TrackFxEnvelope(IntPtr reaperHandle)
  {
    ReaperHandle = reaperHandle;
    int fxIndex = 0, fxParamIndex = 0;
    unsafe
    {
      Track = Track.FromHandle(Reaper.Envelope_GetParentTrack(ReaperHandle, (nint)(&fxIndex), (nint)(&fxParamIndex)));
    }

    FxIndex = fxIndex;
    FxParameterIndex = fxParamIndex;
  }


  private string? GetStringValue(string paramName)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    var value = Marshal.AllocHGlobal(Reaper.NeedBigBufferSize);
    try
    {
      return Reaper.GetSetEnvelopeInfo_String(ReaperHandle, ptr, value, false)
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
      Reaper.GetSetEnvelopeInfo_String(ReaperHandle, ptr, ptrValue, true);
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
      Marshal.FreeHGlobal(ptrValue);
    }
  }

  public ParameterValue GetValue()
  {
    var fx = TrackFx.FromIndex(Track, FxIndex);
    return fx.GetValue(FxParameterIndex);
  }

  public string? GetStateChunk()
  {
    const int bufferSize = 1024 * 1024 * 5;
    var value = Marshal.AllocHGlobal(bufferSize);
    try
    {
      return !Reaper.GetEnvelopeStateChunk(ReaperHandle, value, Reaper.NeedBigBufferSize, false)
        ? null
        : Marshal.PtrToStringAnsi(value);
    }
    finally
    {
      Marshal.FreeHGlobal(value);
    }
  }
}
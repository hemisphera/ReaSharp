using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public class TrackFxEnvelope : ReaperObject
{
  public FxInstanceParameter InstanceParameter { get; }
  public override IntPtr ReaperHandle { get; }

  public Track Track { get; }

  public FxInstance FxInstance { get; }

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

  public int Index { get; }


  public static TrackFxEnvelope FromHandle(nint handle)
  {
    return new TrackFxEnvelope(handle);
  }


  private TrackFxEnvelope(nint reaperHandle)
  {
    ReaperHandle = reaperHandle;
    int fxIndex = 0, fxParamIndex = 0;
    unsafe
    {
      Track = Track.FromHandle(Reaper.Envelope_GetParentTrack(reaperHandle, (nint)(&fxIndex), (nint)(&fxParamIndex)));
    }

    FxInstance = Track.GetFx(fxIndex);
    InstanceParameter = FxInstance.GetParameter(fxParamIndex);
    Index = fxParamIndex;
  }


  public double GetValue(TimeSpan pos)
  {
    double value = 0;
    unsafe
    {
      Reaper.Envelope_Evaluate(
        ReaperHandle, pos.TotalSeconds, 0, 0, (IntPtr)(&value),
        IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }

    return value;
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
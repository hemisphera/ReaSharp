using System.Runtime.InteropServices;

namespace ReaSharp.Models;

public sealed class TrackMediItemTake
{
  public static TrackMediItemTake FromHandle(IntPtr handle)
  {
    return new TrackMediItemTake(handle);
  }


  public string? Name
  {
    get => GetStringValue("P_NAME");
    set => SetStringValue("P_NAME", value);
  }

  public IntPtr ReaperHandle { get; set; }


  private TrackMediItemTake(IntPtr handle)
  {
    ReaperHandle = handle;
  }

  private string? GetStringValue(string paramName)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    var value = Marshal.AllocHGlobal(Reaper.NeedBigBufferSize);
    try
    {
      return Reaper.GetSetMediaItemTakeInfo_String(ReaperHandle, ptr, value, false)
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
    var ptrValue = string.IsNullOrEmpty(value) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(value);
    try
    {
      Reaper.GetSetMediaItemTakeInfo_String(ReaperHandle, ptr, ptrValue, true);
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
      Marshal.FreeHGlobal(ptrValue);
    }
  }

  public byte[]? GetAllMidiEvents()
  {
    var buffer = new byte[1024 * 1024];
    unsafe
    {
      fixed (byte* ptr = buffer)
      {
        var bufferSize = buffer.Length;
        var ok = Reaper.MIDI_GetAllEvts(ReaperHandle, (IntPtr)ptr, (IntPtr)(&bufferSize));
        return ok ? buffer[..bufferSize] : null;
      }
    }
  }

  public bool SetAllMidiEvents(byte[] data)
  {
    unsafe
    {
      fixed (byte* ptr = data)
      {
        return Reaper.MIDI_SetAllEvts(ReaperHandle, (nint)ptr, data.Length);
      }
    }
  }

  public void AddMidiEvent()
  {
    Reaper.MIDI_InsertNote(ReaperHandle, false, false, 0, 10, 1, 100, 100, IntPtr.Zero);
  }
}
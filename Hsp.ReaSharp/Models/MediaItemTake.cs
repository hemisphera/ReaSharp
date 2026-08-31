using System.Runtime.InteropServices;

namespace Hsp.ReaSharp.Models;

public sealed class MediaItemTake : ReaperObject
{
  public static MediaItemTake FromHandle(IntPtr handle)
  {
    return handle == IntPtr.Zero
      ? throw new ArgumentException("Handle cannot be zero.", nameof(handle))
      : new MediaItemTake(handle);
  }


  public string? Name
  {
    get => GetStringValue("P_NAME");
    set => SetStringValue("P_NAME", value);
  }

  public override nint ReaperHandle { get; }

  public int MidiEventCount
  {
    get
    {
      unsafe
      {
        int sysExCount;
        int ccCount;
        int noteCount;
        return Reaper.MIDI_CountEvts.Invoke(ReaperHandle, (nint)(&noteCount), (nint)(&ccCount), (nint)(&sysExCount));
      }
    }
  }


  private MediaItemTake(IntPtr handle)
  {
    ReaperHandle = handle;
  }

  private string? GetStringValue(string paramName)
  {
    var ptr = Marshal.StringToHGlobalAnsi(paramName);
    var value = Marshal.AllocHGlobal(Reaper.NeedBigBufferSize);
    try
    {
      return Reaper.GetSetMediaItemTakeInfo_String.Invoke(ReaperHandle, ptr, value, false)
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
      Reaper.GetSetMediaItemTakeInfo_String.Invoke(ReaperHandle, ptr, ptrValue, true);
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
        var ok = Reaper.MIDI_GetAllEvts.Invoke(ReaperHandle, (IntPtr)ptr, (IntPtr)(&bufferSize));
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
        return Reaper.MIDI_SetAllEvts.Invoke(ReaperHandle, (nint)ptr, data.Length);
      }
    }
  }

  public void AddMidiEvent()
  {
    Reaper.MIDI_InsertNote.Invoke(ReaperHandle, false, false, 0, 10, 1, 100, 100, IntPtr.Zero);
  }

  public MediaItemSource? GetSource()
  {
    var handle = Reaper.GetMediaItemTake_Source.Invoke(ReaperHandle);
    return handle == IntPtr.Zero ? null : new MediaItemSource(handle);
  }

  public void SetSource(MediaItemSource? src)
  {
    var oldSource = GetSource();
    Reaper.SetMediaItemTake_Source.Invoke(ReaperHandle, src?.ReaperHandle ?? IntPtr.Zero);
    if (oldSource != null)
    {
      Marshal.FreeHGlobal(oldSource.ReaperHandle);
    }
  }

  public void DeleteMidiEvent(int index)
  {
    Reaper.MIDI_DeleteEvt.Invoke(ReaperHandle, index);
  }
}
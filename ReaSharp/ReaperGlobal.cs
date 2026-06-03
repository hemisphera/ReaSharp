using System.Runtime.InteropServices;
using ReaSharp.Models;
using ReaSharp.SettingsModels;
using ReaSharp.Utils;

namespace ReaSharp;

public static class ReaperGlobal
{
  public static List<MidiEvent> GetRecentMidiEvents(int? lastSequence = null, int? maxEvents = null)
  {
    maxEvents ??= 32;
    var result = new List<MidiEvent>();

    unsafe
    {
      var buffer = stackalloc byte[1024];
      for (var idx = 0; idx < maxEvents; idx++)
      {
        var bufferSize = 1024;
        var ts = 0;
        var devIdx = 0;
        double projPos = -1;
        var projLoopCnt = 0;

        var seq = Reaper.MIDI_GetRecentInputEvent.Invoke(idx, (IntPtr)buffer, (IntPtr)(&bufferSize), (IntPtr)(&ts), (IntPtr)(&devIdx), (IntPtr)(&projPos), (IntPtr)(&projLoopCnt));
        if (seq == 0) break;
        if (seq <= lastSequence) break;

        if (bufferSize < 1)
          continue;

        var status = buffer[0];
        var data1 = bufferSize > 1 ? buffer[1] : (byte)0;
        var data2 = bufferSize > 2 ? buffer[2] : (byte)0;

        var mevent = new MidiEvent
        {
          BufferSize = bufferSize,
          Sequence = seq,
          Status = status,
          Data1 = data1,
          Data2 = data2,
          DeviceIndex = devIdx & 0xFFFF
        };
        result.Add(mevent);
      }
    }

    return result;
  }

  public static string? GetResourcePath()
  {
    var ptr = Reaper.GetResourcePath.Invoke();
    return Marshal.PtrToStringAnsi(ptr);
  }

  public static IniFile? ReadSettings(string? settingsFilePath = null)
  {
    if (settingsFilePath == null)
    {
      var resourcePath = GetResourcePath();
      if (string.IsNullOrEmpty(resourcePath)) return null;
      settingsFilePath = Path.Combine(resourcePath, "reaper.ini");
    }
    return IniFile.Load(settingsFilePath);
  }

  public static IEnumerable<OscDevice> EnumerateOscDevices(string? settingsFilePath = null)
  {
    var section = ReadSettings(settingsFilePath)?["reaper"];
    if (section == null) return [];
    section.TryGetInt("csurf_cnt", out var surfaceCount);
    List<OscDevice> devices = []; 
    for (var i = 0; i < surfaceCount; i++)
    {
      var line = section["csurf_" + i];
      var dev = OscDevice.Parse(line);
      if (dev != null) devices.Add(dev);
    }

    return devices;
  }
}
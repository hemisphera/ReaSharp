using System.Runtime.InteropServices;

namespace ReaSharp;

public sealed class MidiDevice
{
  public required bool Input { get; init; }
  public required int Id { get; init; }
  public required string Name { get; init; }
  public required string Alias { get; init; }


  private MidiDevice()
  {
  }


  public static List<MidiDevice> EnumerateInputs()
  {
    var resp = new List<MidiDevice>();
    unsafe
    {
      const int bufferSize = 255;
      var buf = stackalloc byte[bufferSize];
      for (var i = 0; i < Reaper.GetNumMIDIInputs(); i++)
      {
        if (!Reaper.GetMIDIInputName(i, (IntPtr)buf, bufferSize)) continue;
        var alias = Marshal.PtrToStringAnsi((IntPtr)buf) ?? string.Empty;

        Reaper.GetMIDIInputNameNoAlias(i, (IntPtr)buf, bufferSize);
        var name = Marshal.PtrToStringAnsi((IntPtr)buf) ?? string.Empty;

        resp.Add(new MidiDevice
        {
          Input = true,
          Id = i,
          Alias = alias,
          Name = name
        });
      }
    }

    return resp;
  }

  public static List<MidiDevice> EnumerateOutput()
  {
    var resp = new List<MidiDevice>();
    unsafe
    {
      const int bufferSize = 255;
      var buf = stackalloc byte[bufferSize];
      for (var i = 0; i < Reaper.GetNumMIDIOutputs(); i++)
      {
        if (!Reaper.GetMIDIOutputName(i, (IntPtr)buf, bufferSize)) continue;
        var alias = Marshal.PtrToStringAnsi((IntPtr)buf) ?? string.Empty;

        Reaper.GetMIDIOutputNameNoAlias(i, (IntPtr)buf, bufferSize);
        var name = Marshal.PtrToStringAnsi((IntPtr)buf) ?? string.Empty;

        resp.Add(new MidiDevice
        {
          Input = false,
          Id = i,
          Alias = alias,
          Name = name
        });
      }
    }

    return resp;
  }


  public override string ToString()
  {
    var type = Input ? "IN" : "OUT";
    return $"{Id}: {Alias} ({Name}) ({type})";
  }
}
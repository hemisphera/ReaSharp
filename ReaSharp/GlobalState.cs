namespace ReaSharp;

public static class GlobalState
{
  public static MidiDevice[] InputDevices { get; private set; } = [];
  public static MidiDevice[] OutputDevices { get; private set; } = [];


  public static void Initialize()
  {
    InputDevices = MidiDevice.EnumerateInputs().ToArray();
    OutputDevices = MidiDevice.EnumerateOutput().ToArray();
  }
}
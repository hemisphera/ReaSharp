namespace ReaSharp.Models;

public enum RecordingMode
{
  Input = 0,
  StereoOutput = 1,
  None = 2,
  StereoOutputWithLatencyComp = 3,
  MidiOutput = 4,
  MonoOutput = 5,
  MonoOutputWithLatencyComp = 6,
  MidiOverdub = 7,
  MidiReplace = 8
}
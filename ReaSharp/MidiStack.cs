namespace ReaSharp;

public static class MidiStack
{
  public static void HandleInboundEvent(MidiEvent mevent)
  {
    Plugin.ReaperLog(
      $"MIDI in: seq={mevent.Sequence} dev={mevent.DeviceIndex} len={mevent.BufferSize} msg={mevent.Status} {mevent.Data1} {mevent.Data2}");
  }
}
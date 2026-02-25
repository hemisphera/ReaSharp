using ReaSharp.Models;

namespace ReaSharp;

public static class MidiStack
{
  public static void HandleInboundEvent(MidiEvent mevent)
  {
    ReaperLogger.Log(
      $"MIDI in: seq={mevent.Sequence} dev={mevent.DeviceIndex} len={mevent.BufferSize} msg={mevent.Status} {mevent.Data1} {mevent.Data2}");
  }
}
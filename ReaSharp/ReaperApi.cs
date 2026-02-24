namespace ReaSharp;

public static class ReaperApi
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

        var seq = Reaper.MIDI_GetRecentInputEvent(idx, buffer, &bufferSize, &ts, &devIdx, &projPos, &projLoopCnt);
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
}
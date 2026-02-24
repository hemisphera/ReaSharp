using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ReaSharp;

public sealed class MainLoop
{
  //private readonly IntPtr _timerFuncPtr;
  //private const string TimerName = "ReaSharpTimer";
  //private readonly CancellationTokenSource _cts;

  private readonly MidiListener _listener;

  public MainLoop()
  {
    _listener = new MidiListener(MidiCallback);

    foreach (var dev in MidiDevice.EnumerateOutput())
    {
      ReaperLogger.Log(dev.ToString());
    }

    foreach (var dev in MidiDevice.EnumerateInputs())
    {
      ReaperLogger.Log(dev.ToString());
    }
  }

  private void MidiCallback(MidiEvent me)
  {
    //ReaperLogger.Log(me.ToString());
  }

  /*

private unsafe IntPtr RegisterTimer()
{
  var funcPtr = (IntPtr)(delegate* unmanaged[Cdecl]<void>)&TimerTick;
  var timerName = Marshal.StringToHGlobalAnsi(TimerName);
  Reaper.Register(timerName, _timerFuncPtr);
  Marshal.FreeHGlobal(timerName);
  return funcPtr;
}


[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
private static unsafe void TimerTick()
{
  const int maxToProcess = 32;
  var processed = 0;
  var buffer = stackalloc byte[1024];

  for (var idx = 0; idx < maxToProcess; idx++)
  {
    var bufferSize = 1024;
    var ts = 0;
    var devIdx = 0;
    double projPos = -1;
    var projLoopCnt = 0;

    var seq = _midiGetRecentInputEvent(
      idx,
      buffer,
      &bufferSize,
      &ts,
      &devIdx,
      &projPos,
      &projLoopCnt);

    if (seq == 0)
      break;

    if (seq == _lastMidiSequence)
      break;

    if (bufferSize < 1)
      continue;

    var status = buffer[0];
    var data1 = bufferSize > 1 ? buffer[1] : (byte)0;
    var data2 = bufferSize > 2 ? buffer[2] : (byte)0;

    var mevent = new MidiEvent
    {
      BufferSize = bufferSize,
      Sequence = seq,
      Data1 = data1,
      Data2 = data2,
      DeviceIndex = devIdx & 0xFFFF
    };
    MidiStack.HandleInboundEvent(mevent);

    if (idx == 0)
      _lastMidiSequence = seq;

    processed++;
  }

  if (processed == 0)
    return;
}

public void Dispose()
{
  _cts.Cancel();
  _cts.Dispose();

  if (_timerFuncPtr == IntPtr.Zero)
    return;

  var timerName = Marshal.StringToHGlobalAnsi($"-{TimerName}");
  Reaper.Register(timerName, _timerFuncPtr);
  Marshal.FreeHGlobal(timerName);
}
  */
}
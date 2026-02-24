using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace ReaSharp;

public static class Plugin
{
  // REAPER_PLUGIN_VERSION as defined in reaper_plugin.h
  private const int REAPER_PLUGIN_VERSION = 0x20E;

  // 1. Define a "Delegate" that matches the C++ signature of the REAPER function
  // In C++, this is: void (*ShowConsoleMsg)(const char* msg)
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate void ShowConsoleMsgDelegate(IntPtr msg);

  // GetFunc takes a const char* and returns a void* (function pointer)
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate IntPtr GetFuncDelegate(IntPtr name);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate int RegisterDelegate(IntPtr name, IntPtr infoStruct);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public unsafe delegate int MidiGetRecentInputEventDelegate(
    int idx,
    byte* bufOut,
    int* bufOutSz,
    int* tsOut,
    int* devIdxOut,
    double* projPosOut,
    int* projLoopCntOut);

  private static ShowConsoleMsgDelegate? _showConsoleMsg;
  private static RegisterDelegate? _register;
  private static MidiGetRecentInputEventDelegate? _midiGetRecentInputEvent;
  private static IntPtr _timerFuncPtr = IntPtr.Zero;
  private static int _lastMidiSequence;
  private static bool _timerRegistered;

  [UnmanagedCallersOnly(EntryPoint = "ReaperPluginEntry")]
  public static unsafe int ReaperPluginEntry(IntPtr hInstance, IntPtr rec)
  {
    try
    {
      if (rec == IntPtr.Zero)
      {
        UnregisterTimer();
        return 0;
      }

      var info = Marshal.PtrToStructure<ReaperPluginInfo>(rec);

      if (info.GetFunc == IntPtr.Zero || info.CallerVersion != REAPER_PLUGIN_VERSION)
        return 0;

      _register = Marshal.GetDelegateForFunctionPointer<RegisterDelegate>(info.Register);
      var getFunc = Marshal.GetDelegateForFunctionPointer<GetFuncDelegate>(info.GetFunc);

      // Use IntPtr-based string to avoid managed marshalling issues in AOT
      var funcName = Marshal.StringToHGlobalAnsi("ShowConsoleMsg");
      var consolePtr = getFunc(funcName);
      Marshal.FreeHGlobal(funcName);

      if (consolePtr == IntPtr.Zero) return 1; // loaded, but func not found — still return 1

      _showConsoleMsg = Marshal.GetDelegateForFunctionPointer<ShowConsoleMsgDelegate>(consolePtr);

      var midiFuncName = Marshal.StringToHGlobalAnsi("MIDI_GetRecentInputEvent");
      var midiPtr = getFunc(midiFuncName);
      Marshal.FreeHGlobal(midiFuncName);

      if (midiPtr != IntPtr.Zero)
      {
        _midiGetRecentInputEvent = Marshal.GetDelegateForFunctionPointer<MidiGetRecentInputEventDelegate>(midiPtr);
        RegisterTimer();
        ReaperLog("ReaSharp MIDI polling enabled.\n");
      }
      else
      {
        ReaperLog("ReaSharp: MIDI_GetRecentInputEvent unavailable.\n");
      }

      ReaperLog("Hello from ReaSharp (Native AOT)!\n");

      return 1; // success
    }
    catch (Exception ex)
    {
      return 0;
    }
  }

  private static unsafe void RegisterTimer()
  {
    if (_register == null || _timerRegistered)
      return;

    if (_timerFuncPtr == IntPtr.Zero)
      _timerFuncPtr = (IntPtr)(delegate* unmanaged[Cdecl]<void>)&TimerTick;

    var timerName = Marshal.StringToHGlobalAnsi("timer");
    _register(timerName, _timerFuncPtr);
    Marshal.FreeHGlobal(timerName);
    _timerRegistered = true;
  }

  private static void UnregisterTimer()
  {
    if (_register == null || !_timerRegistered || _timerFuncPtr == IntPtr.Zero)
      return;

    var timerName = Marshal.StringToHGlobalAnsi("-timer");
    _register(timerName, _timerFuncPtr);
    Marshal.FreeHGlobal(timerName);
    _timerRegistered = false;
  }

  [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
  private static unsafe void TimerTick()
  {
    if (_midiGetRecentInputEvent == null)
      return;

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

  public static void ReaperLog(string text)
  {
    if (_showConsoleMsg == null)
      return;

    var msg = Marshal.StringToHGlobalAnsi(text + "\n");
    _showConsoleMsg(msg);
    Marshal.FreeHGlobal(msg);
  }
}
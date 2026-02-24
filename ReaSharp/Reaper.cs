using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace ReaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr GetFuncDelegate(IntPtr name);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int RegisterDelegate(IntPtr name, IntPtr infoStruct);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ShowConsoleMsgDelegate(IntPtr msg);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int MIDI_GetRecentInputEventDelegate(int idx, byte* bufOut, int* bufOutSz, int* tsOut, int* devIdxOut, double* projPosOut, int* projLoopCntOut);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate bool GetMIDIInputNameDelegate(int dev, byte* nameout, int nameout_sz);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate bool GetMIDIOutputNameDelegate(int dev, byte* nameout, int nameout_sz);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int GetNumMIDIInputsDelegate();

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int GetNumMIDIOutputsDelegate();

public static class Reaper
{
  public static GetFuncDelegate GetFunc { get; private set; } = null!;
  public static RegisterDelegate Register { get; set; } = null!;
  public static ShowConsoleMsgDelegate ShowConsoleMsg { get; private set; } = null!;
  public static MIDI_GetRecentInputEventDelegate MIDI_GetRecentInputEvent { get; private set; } = null!;
  public static GetMIDIInputNameDelegate GetMIDIInputName { get; private set; } = null!;
  public static GetMIDIInputNameDelegate GetMIDIInputNameNoAlias { get; private set; } = null!;
  public static GetMIDIOutputNameDelegate GetMIDIOutputName { get; private set; } = null!;
  public static GetMIDIOutputNameDelegate GetMIDIOutputNameNoAlias { get; private set; } = null!;
  public static GetNumMIDIInputsDelegate GetNumMIDIInputs { get; private set; } = null!;
  public static GetNumMIDIOutputsDelegate GetNumMIDIOutputs { get; private set; } = null!;


  public static void LoadFunctions(ReaperPluginInfo pluginInfo)
  {
    GetFunc = Marshal.GetDelegateForFunctionPointer<GetFuncDelegate>(pluginInfo.GetFunc);
    Register = Marshal.GetDelegateForFunctionPointer<RegisterDelegate>(pluginInfo.Register);
    ShowConsoleMsg = LoadFunction<ShowConsoleMsgDelegate>(nameof(ShowConsoleMsg));
    MIDI_GetRecentInputEvent = LoadFunction<MIDI_GetRecentInputEventDelegate>(nameof(MIDI_GetRecentInputEvent));
    GetMIDIInputName = LoadFunction<GetMIDIInputNameDelegate>(nameof(GetMIDIInputName));
    GetMIDIOutputName = LoadFunction<GetMIDIOutputNameDelegate>(nameof(GetMIDIOutputName));
    GetMIDIInputNameNoAlias = LoadFunction<GetMIDIInputNameDelegate>(nameof(GetMIDIInputNameNoAlias));
    GetMIDIOutputNameNoAlias = LoadFunction<GetMIDIOutputNameDelegate>(nameof(GetMIDIOutputNameNoAlias));
    GetNumMIDIInputs = LoadFunction<GetNumMIDIInputsDelegate>(nameof(GetNumMIDIInputs));
    GetNumMIDIOutputs = LoadFunction<GetNumMIDIOutputsDelegate>(nameof(GetNumMIDIOutputs));
  }


  private static T LoadFunction<T>(string name) where T : Delegate
  {
    var funcName = Marshal.StringToHGlobalAnsi(name);
    var funcPtr = GetFunc.Invoke(funcName);
    Marshal.FreeHGlobal(funcName);
    return funcPtr == IntPtr.Zero
      ? throw new Exception($"Unable to load function '{name}'.")
      : Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
  }
}
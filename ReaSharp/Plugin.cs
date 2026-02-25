using System.Runtime.InteropServices;
using ReaSharp.Models;

namespace ReaSharp;

public static class Plugin
{
  // REAPER_PLUGIN_VERSION as defined in reaper_plugin.h
  private const int ReaperPluginVersion = 0x20E;

  public static MainLoop Loop { get; set; } = null!;


  [UnmanagedCallersOnly(EntryPoint = "ReaperPluginEntry")]
  public static unsafe int ReaperPluginEntry(IntPtr hInstance, IntPtr rec)
  {
    try
    {
      if (rec == IntPtr.Zero)
      {
        return 0;
      }

      var info = Marshal.PtrToStructure<ReaperPluginInfo>(rec);

      if (info.GetFunc == IntPtr.Zero || info.CallerVersion != ReaperPluginVersion)
        return 0;

      Reaper.LoadFunctions(info);

      GlobalState.Initialize();
      CommandRegistry.Initialize();
      CommandRegistry.Register("REASHARP_TEST1", "ReaSharp: Test 1", () => _ = CommandRegistry.RunTest1());
      CommandRegistry.Register("REASHARP_TEST2", "ReaSharp: Test 2", () => _ = CommandRegistry.RunTest2());

      ReaperLogger.Log($"REAPER version: {info.CallerVersion}");

      return 1; // success
    }
    catch
    {
      return 0;
    }
  }
}
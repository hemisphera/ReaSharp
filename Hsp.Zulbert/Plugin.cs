using System.Runtime.InteropServices;
using ReaSharp;

namespace Hsp.Zulbert;

public static class Plugin
{
  [UnmanagedCallersOnly(EntryPoint = "ReaperPluginEntry")]
  public static int ReaperPluginEntry(IntPtr hInstance, IntPtr rec)
  {
    try
    {
      PluginState.Initialize(ReaperPluginInfo.FromPointer(rec));
      var cr = PluginState.Instance.AddCommandRegistry(new DefaultCommandRegistry());

      ReaperLogger.Log("Zulbert is dere, mon!");
      cr.Register("REASHARP_TEST1", "ReaSharp: Test 1", () => _ = Commands.RunTest1());
      cr.Register("REASHARP_TEST2", "ReaSharp: Test 2", () => _ = Commands.RunTest2());

      return 1;
    }
    catch
    {
      return 0;
    }
  }
}
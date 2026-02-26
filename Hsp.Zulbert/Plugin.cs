using System.Runtime.InteropServices;
using ReaSharp;

namespace Hsp.Zulbert;

public static class Plugin
{
  public static ZulState State { get; private set; } = null!;

  [UnmanagedCallersOnly(EntryPoint = "ReaperPluginEntry")]
  public static int ReaperPluginEntry(IntPtr hInstance, IntPtr rec)
  {
    try
    {
      var state = PluginState.Initialize(ReaperPluginInfo.FromPointer(rec));
      var cr = state.AddCommandRegistry(new DefaultCommandRegistry());
      cr.Register("ZULBERT_PLAY", "Zulbert: Play", () => _ = Commands.Play());
      cr.Register("ZULBERT_WATCH", "Zulbert: Watch", () => _ = Commands.Watch());
      state.Gmem.Connect("RPLT_MEM");

      ReaperLogger.LogInformation("Initialized");

      return 1;
    }
    catch
    {
      return 0;
    }
  }
}
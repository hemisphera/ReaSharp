using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ReaSharp;

public class PluginState
{
  private static PluginState? _instance;

  public static PluginState Instance => _instance ?? throw new Exception("Plugin state not initialized.");


  public ICommandRegistry? Commands { get; private set; }


  public static PluginState Initialize(ReaperPluginInfo pluginInfo)
  {
    _instance = new PluginState(pluginInfo);
    return _instance;
  }


  private PluginState(ReaperPluginInfo pluginInfo)
  {
    Reaper.LoadFunctions(pluginInfo);
  }


  public ICommandRegistry AddCommandRegistry(ICommandRegistry reg)
  {
    if (Commands != null) throw new Exception("Command registry has already been set");
    Commands = reg;

    unsafe
    {
      var hookPtr = (IntPtr)(delegate* unmanaged[Cdecl]<int, int, bool>)&HookCommand;
      var hookName = Marshal.StringToHGlobalAnsi("hookcommand");
      Reaper.Register(hookName, hookPtr);
      Marshal.FreeHGlobal(hookName);
    }

    return reg;
  }

  /// <summary>
  /// Called by REAPER for every command execution in the main section.
  /// Returns true to indicate the command was handled (preventing further processing).
  /// </summary>
  [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
  private static bool HookCommand(int command, int flag)
  {
    ReaperLogger.LogDebug($"Finding command {command}");
    var cmd = Instance.Commands?.GetById(command);
    if (cmd == null) return false;
    ReaperLogger.LogDebug("Found. Running.");
    cmd.Execute();
    return true;
  }
}
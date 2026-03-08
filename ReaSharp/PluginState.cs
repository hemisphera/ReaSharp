using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReaSharp;

public class PluginState
{
  private static PluginState? _instance;

  //private static ReaperSynchronizationContext? _syncContext;
  private readonly IHost _host;

  public static PluginState Instance => _instance ?? throw new Exception("Plugin state not initialized.");
  //public static ReaperSynchronizationContext SyncContext => _syncContext ?? throw new Exception("Plugin state not initialized.");

  public ICommandRegistry? Commands => Services.GetService<ICommandRegistry>();
  public IServiceProvider Services => _host.Services;


  public static PluginState Initialize(ReaperPluginInfo pluginInfo, IHost host)
  {
    // Establish the main-thread SynchronizationContext before building the host so that
    // any async continuations awaited from plugin code are marshaled back here.
    //_syncContext = new ReaperSynchronizationContext();
    //SynchronizationContext.SetSynchronizationContext(_syncContext);

    _instance = new PluginState(pluginInfo, host);
    ConfigureHookCommand();
    //ConfigureTimer();
    return _instance;
  }


  private PluginState(ReaperPluginInfo pluginInfo, IHost host)
  {
    _host = host;
    Reaper.LoadFunctions(pluginInfo);
    _host.Start();
  }


  private static void ConfigureHookCommand()
  {
    unsafe
    {
      var hookPtr = (IntPtr)(delegate* unmanaged[Cdecl]<int, int, bool>)&HookCommand;
      var hookName = Marshal.StringToHGlobalAnsi("hookcommand");
      Reaper.Register(hookName, hookPtr);
      Marshal.FreeHGlobal(hookName);
    }
  }

  /*
  private static void ConfigureTimer()
  {
    unsafe
    {
      var timerPtr = (IntPtr)(delegate* unmanaged[Cdecl]<void>)&TimerCallback;
      var timerName = Marshal.StringToHGlobalAnsi("timer");
      Reaper.Register(timerName, timerPtr);
      Marshal.FreeHGlobal(timerName);
    }
  }

  [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
  private static void TimerCallback()
  {
    _syncContext?.ProcessQueue();
  }
  */

  /// <summary>
  /// Called by REAPER for every command execution in the main section.
  /// Returns true to indicate the command was handled (preventing further processing).
  /// </summary>
  [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
  private static bool HookCommand(int command, int flag)
  {
    var cmd = Instance.Commands?.GetById(command);
    if (cmd == null) return false;
    cmd.Execute(Instance.Services);
    return true;
  }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReaSharp;

public class PluginState
{
  private static PluginState? _instance;
  private readonly IHost _host;

  public static PluginState Instance => _instance ?? throw new Exception("Plugin state not initialized.");

  public ICommandRegistry? Commands => Services.GetService<ICommandRegistry>();
  public IServiceProvider Services => _host.Services;


  public static PluginState Initialize(
    ReaperPluginInfo pluginInfo,
    Action<HostBuilderContext, IServiceCollection>? servicesCallback = null,
    Action<HostBuilderContext, ILoggingBuilder>? loggingCallback = null)
  {
    var hostBuilder = Host.CreateDefaultBuilder();
    if (servicesCallback != null)
      hostBuilder.ConfigureServices(servicesCallback);
    if (loggingCallback != null)
      hostBuilder.ConfigureLogging(loggingCallback);
    _instance = new PluginState(pluginInfo, hostBuilder.Build());
    ConfigureHookCommand();
    return _instance;
  }


  private PluginState(ReaperPluginInfo pluginInfo, IHost host)
  {
    _host = host;
    Reaper.LoadFunctions(pluginInfo);
    _host.Start();
    ReaperConsoleLogger.WriteLog("Plugin initialized.");
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
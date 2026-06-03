using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReaSharp;

public class PluginState
{
  private static PluginState? _instance;

  private readonly IHost _host;

  public static PluginState Instance => _instance ?? throw new Exception("Plugin state not initialized.");

  public ICommandRegistry? Commands => Services.GetService<ICommandRegistry>();
  public IServiceProvider Services => _host.Services;


  public static PluginState Initialize(ReaperPluginInfo pluginInfo, IHost host)
  {
    _instance = new PluginState(pluginInfo, host);
    ConfigureHookCommand();
    return _instance;
  }


  public ICommandRegistry EnsureCommandRegistry()
  {
    return Commands ?? throw new Exception("No command registry specified.");
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
      // hookcommand2: bool onAction(KbdSectionInfo *sec, int command, int val, int val2, int relmode, HWND hwnd)
      var hookPtr = (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, int, int, int, int, IntPtr, bool>)&HookCommand;
      var hookName = Marshal.StringToHGlobalAnsi("hookcommand2");
      Reaper.Register(hookName, hookPtr);
      Marshal.FreeHGlobal(hookName);
    }
  }

  /// <summary>
  /// Called by REAPER before every action triggered by a key, MIDI, or OSC event.
  /// Returns true to indicate the command was handled (preventing further hooks or the action from running).
  /// </summary>
  /// <param name="sectionPtr">Pointer to KbdSectionInfo (section context).</param>
  /// <param name="command">Action command ID.</param>
  /// <param name="val">MIDI/OSC value component [0..127].</param>
  /// <param name="val2">-1 for MIDI CC; &gt;=0 for MIDI pitch or OSC. OSC float = (val2|(val&lt;&lt;7))/16383.0</param>
  /// <param name="relMode">0=absolute, 1/2/3=relative adjust modes.</param>
  /// <param name="hwnd">Section window handle; zero for MIDI/OSC triggers.</param>
  [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
  private static bool HookCommand(IntPtr sectionPtr, int command, int val, int val2, int relMode, IntPtr hwnd)
  {
    var cmd = Instance.Commands?.GetById(command);
    if (cmd == null) return false;

    var section = sectionPtr != IntPtr.Zero
      ? Marshal.PtrToStructure<Models.KbdSectionInfo>(sectionPtr)
      : default;

    var context = new Models.ActionContext
    {
      SectionId = section.UniqueId,
      Val = val,
      Val2 = val2,
      RelMode = relMode
    };

    cmd.Execute(Instance.Services, context);
    return true;
  }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ReaSharp;

/// <summary>
/// Registers REAPER commands (gaccel) and dispatches execution via hookcommand.
/// Must call <see cref="Initialize"/> once after <see cref="Reaper.LoadFunctions"/> before registering any commands.
/// </summary>
public static class CommandRegistry
{
  private static readonly Dictionary<int, ReaperCommand> Commands = [];

  // Unmanaged memory that must outlive the plugin — never freed.
  private static readonly List<IntPtr> PinnedAllocations = [];

  /// <summary>
  /// Registers the hookcommand callback with REAPER. Must be called once during plugin startup.
  /// </summary>
  public static unsafe void Initialize()
  {
    var hookPtr = (IntPtr)(delegate* unmanaged[Cdecl]<int, int, bool>)&HookCommand;
    var hookName = Marshal.StringToHGlobalAnsi("hookcommand");
    Reaper.Register(hookName, hookPtr);
    Marshal.FreeHGlobal(hookName);
  }

  /// <summary>
  /// Registers a command with REAPER and returns the resulting <see cref="ReaperCommand"/>
  /// whose <see cref="ReaperCommand.Id"/> is assigned by REAPER.
  /// </summary>
  /// <param name="uniqueName">Stable, unique identifier for this command (e.g. "ReaSharp_MyAction"). Must not change between sessions.</param>
  /// <param name="description">Label shown in the REAPER Action List.</param>
  /// <param name="execute">Callback invoked when the command is triggered.</param>
  public static ReaperCommand Register(string uniqueName, string description, Action execute)
  {
    // Step 1: register the named command ID — return value IS the assigned command ID.
    var uniqueNamePtr = Marshal.StringToHGlobalAnsi(uniqueName);
    PinnedAllocations.Add(uniqueNamePtr); // must remain alive
    var cmdIdName = Marshal.StringToHGlobalAnsi("command_id");
    var commandId = Reaper.Register(cmdIdName, uniqueNamePtr);
    Marshal.FreeHGlobal(cmdIdName);

    ReaperLogger.LogDebug($"Registered command ID {commandId}");

    if (commandId == 0)
      throw new Exception($"REAPER returned command ID 0 for '{uniqueName}'. Check that the name is unique.");

    // Step 2: register a gaccel_register_t so the command appears in the Action List.
    var descPtr = Marshal.StringToHGlobalAnsi(description);
    PinnedAllocations.Add(descPtr);

    // The struct must remain alive — REAPER holds the pointer.
    var structPtr = Marshal.AllocHGlobal(Marshal.SizeOf<GaccelRegister>());
    PinnedAllocations.Add(structPtr);

    var reg = new GaccelRegister
    {
      Accel = new Accel { fVirt = 0, key = 0, cmd = (ushort)commandId },
      Desc = descPtr
    };
    Marshal.StructureToPtr(reg, structPtr, false);

    var regName = Marshal.StringToHGlobalAnsi("gaccel");
    var gaccelResult = Reaper.Register(regName, structPtr);
    Marshal.FreeHGlobal(regName);

    if (gaccelResult == 0)
      throw new Exception($"REAPER rejected gaccel registration for '{uniqueName}'.");

    var command = new ReaperCommand
    {
      Id = commandId,
      Description = description,
      Execute = execute
    };

    Commands[commandId] = command;
    return command;
  }

  /// <summary>
  /// Called by REAPER for every command execution in the main section.
  /// Returns true to indicate the command was handled (preventing further processing).
  /// </summary>
  [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
  private static bool HookCommand(int command, int flag)
  {
    if (!Commands.TryGetValue(command, out var cmd))
      return false;

    cmd.Execute();
    return true;
  }
}
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using ReaSharp.Models;

namespace ReaSharp;

public class DefaultCommandRegistry : ICommandRegistry
{
  private readonly ILogger<DefaultCommandRegistry> _logger;
  private static readonly Dictionary<int, ReaperCommand> Commands = [];

  // Unmanaged memory that must outlive the plugin — never freed.
  private static readonly List<IntPtr> PinnedAllocations = [];

  public DefaultCommandRegistry(ILogger<DefaultCommandRegistry> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Registers a command with REAPER and returns the resulting <see cref="ReaperCommand"/>
  /// whose <see cref="ReaperCommand.Id"/> is assigned by REAPER.
  /// </summary>
  /// <param name="uniqueName">Stable, unique identifier for this command (e.g. "ReaSharp_MyAction"). Must not change between sessions.</param>
  /// <param name="description">Label shown in the REAPER Action List.</param>
  /// <param name="handler">Callback invoked when the command is triggered.</param>
  public ReaperCommand Register(string uniqueName, string description, Func<IServiceProvider, Task> handler)
  {
    // Step 1: register the named command ID — return value IS the assigned command ID.
    var uniqueNamePtr = Marshal.StringToHGlobalAnsi(uniqueName);
    PinnedAllocations.Add(uniqueNamePtr); // must remain alive
    var cmdIdName = Marshal.StringToHGlobalAnsi("command_id");
    var commandId = Reaper.Register(cmdIdName, uniqueNamePtr);
    Marshal.FreeHGlobal(cmdIdName);

    _logger.LogDebug("Registered command ID {commandId}", commandId);

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
      Handler = handler
    };

    Commands[commandId] = command;
    return command;
  }

  public ReaperCommand? GetById(int command)
  {
    return Commands.GetValueOrDefault(command);
  }

  /*
  public static async Task RunTest1()
  {
    var sw = Stopwatch.StartNew();
    var tracks = Track.Enumerate().ToList();
    sw.Stop();
    ReaperLogger.LogDebug($"Enumerate tracks: {sw.Elapsed.TotalMilliseconds}ms");

    var lastTrack = tracks.Last();

    sw = Stopwatch.StartNew();
    var isMuted = lastTrack.Mute;
    sw.Stop();
    ReaperLogger.LogDebug($"Reading mute: {sw.Elapsed.TotalMilliseconds}ms");

    sw = Stopwatch.StartNew();
    lastTrack.Mute = !isMuted;
    ReaperLogger.LogDebug($"Toggle mute: {sw.Elapsed.TotalMilliseconds}ms");

    sw = Stopwatch.StartNew();
    lastTrack.Mute = !isMuted;
    ReaperLogger.LogDebug($"Toggle same mute: {sw.Elapsed.TotalMilliseconds}ms");

    sw = Stopwatch.StartNew();
    foreach (var track in tracks)
    {
      track.Mute = true;
    }

    ReaperLogger.LogDebug($"Toggle mute on all: {sw.Elapsed.TotalMilliseconds}ms");
  }

  public static async Task RunTest2()
  {
    var tracks = Track.Enumerate();
    int level = 0;
    foreach (var track in tracks)
    {
      var msg = "".PadLeft(level) + track.Name;
      level += track.FolderLevel;

      ReaperLogger.Log(msg);
      foreach (var item in track.EnumerateMediaItems())
      {
        ReaperLogger.Log($"{item.Index}");
      }
    }
  }
  */
}
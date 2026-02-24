namespace ReaSharp;

/// <summary>Represents a REAPER command registered via gaccel_register and visible in the Action List.</summary>
public sealed class ReaperCommand
{
  /// <summary>The command ID assigned by REAPER (sourced from accel.cmd after registration).</summary>
  public required int Id { get; init; }

  /// <summary>Human-readable description shown in the REAPER Action List.</summary>
  public required string Description { get; init; }

  /// <summary>The callback invoked when the command is executed.</summary>
  public required Action Execute { get; init; }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ReaSharp.Models;

/// <summary>Represents a REAPER command registered via gaccel_register and visible in the Action List.</summary>
public sealed class ReaperCommand
{
  /// <summary>The command ID assigned by REAPER (sourced from accel.cmd after registration).</summary>
  public required int Id { get; init; }

  /// <summary>Human-readable description shown in the REAPER Action List.</summary>
  public required string Description { get; init; }

  public required Func<IServiceProvider, Task> Handler { get; init; }


  public void Execute(IServiceProvider services)
  {
    _ = Task.Run(async () =>
    {
      using var scope = services.CreateScope();
      var logger = scope.ServiceProvider.GetService<ILogger<ReaperCommand>>();
      try
      {
        await Handler(scope.ServiceProvider);
      }
      catch (Exception exception)
      {
        logger?.LogError(exception, "Command execution failed: {msg}\n{stack}", exception.Message, exception.StackTrace);
      }
    });
  }
}
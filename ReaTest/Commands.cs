using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReaSharp.Models;

namespace ReaTest;

public static class Commands
{
  public static async Task RunTests(IServiceProvider provider)
  {
    var logger = provider.GetRequiredService<ILogger<ReaperCommand>>();
    var project = Project.Current;
    var selectedTrack = project.GetSelectedTrack();
    if (selectedTrack == null) return;
    logger.LogInformation($"Selected track: {selectedTrack}");

    var fxInstances = selectedTrack.EnumerateFx().ToList();
    logger.LogInformation($"FX count: {fxInstances.Count}");
    foreach (var fxInstance in fxInstances)
    {
      var paramCount = fxInstance.ParameterCount;
      logger.LogInformation($"> Param count: {paramCount}");
      Enumerable.Range(0, paramCount).ToList().ForEach(i => LogParameter(fxInstance.GetParameter(i), logger));
    }
  }

  private static void LogParameter(FxInstanceParameter parameter, ILogger logger)
  {
    logger.LogInformation($"Parameter #{parameter.Index} {parameter.Name}");
    logger.LogInformation($" > Value : {parameter.GetValue()}");
    logger.LogInformation($"           {parameter.GetFormattedValue()}");
    logger.LogInformation($" > Range : {parameter.Minimum} -  {parameter.Maximum}");
    logger.LogInformation($" > Steps : {parameter.StepSize}");
    logger.LogInformation($" > Toggle: {parameter.IsToggle}");
  }
}
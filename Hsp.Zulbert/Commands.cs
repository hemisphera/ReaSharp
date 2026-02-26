using ReaSharp;

namespace Hsp.Zulbert;

public static class Commands
{
  public static async Task Play()
  {
    ReaperLogger.LogInformation("Play");
    /*
    var state = await ZulState.Create();
    if (state == null) return;
    await state.Run();
    */
  }

  public static async Task RunTest2()
  {
  }

  public static async Task Watch()
  {
    ReaperLogger.LogInformation("Starting watch");
    while (true)
    {
      await Task.Delay(1000);
      //var value = PluginState.Instance.Gmem.Read(0);
      PluginState.Instance.Gmem.Write(0, 0.5);
      //ReaperLogger.LogInformation($"{value}");
    }
  }
}
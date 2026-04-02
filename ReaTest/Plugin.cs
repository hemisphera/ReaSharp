using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReaSharp;
using ReaSharp.Models;

namespace ReaTest;

public static class Plugin
{
  [UnmanagedCallersOnly(EntryPoint = "ReaperPluginEntry")]
  public static int ReaperPluginEntry(IntPtr hInstance, IntPtr rec)
  {
    try
    {
      var host = BuildHost();
      PluginState.Initialize(ReaperPluginInfo.FromPointer(rec), host);
      var commands = PluginState.Instance.EnsureCommandRegistry();
      commands.Register("REATEST_TEST", "REATEST: Test", Commands.RunTests);
      var logger = host.Services.GetRequiredService<ILogger<ReaperCommand>>();
      return 1;
    }
    catch
    {
      // nay!
      return 0;
    }
  }

  private static IHost BuildHost()
  {
    var host = Host.CreateDefaultBuilder()
      .ConfigureLogging((context, lb) =>
      {
        lb.ClearProviders();
        lb.SetMinimumLevel(LogLevel.Error);
        lb.AddFilter((category, level) => category != null && category.StartsWith("ReaSharp") ? level >= LogLevel.Information : level >= LogLevel.Error);
        lb.AddConfiguration(context.Configuration.GetSection("Logging"));
        lb.AddProvider(new ReaperConsoleLoggerProvider());
      })
      .ConfigureServices((context, sc) => { sc.AddSingleton<ICommandRegistry, DefaultCommandRegistry>(); })
      .Build();
    return host;
  }
}
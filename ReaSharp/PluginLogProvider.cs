using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace ReaSharp;

public sealed class PluginLogProvider : ILoggerProvider
{
  public ILogger CreateLogger(string categoryName) => new PluginLogger();

  public void Dispose()
  {
  }

  private sealed class PluginLogger : ILogger
  {
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoopScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
      var prefix = logLevel switch
      {
        LogLevel.Trace => "[TRCE]",
        LogLevel.Debug => "[DEBG]",
        LogLevel.Warning => "[WARN]",
        LogLevel.Error => "[FAIL]",
        LogLevel.Critical => "[CRIT]",
        _ => "[INFO]"
      };
      var msgData = Marshal.StringToHGlobalAnsi(prefix + " " + formatter(state, exception) + "\n");
      Reaper.ShowConsoleMsg(msgData);
      Marshal.FreeHGlobal(msgData);
    }

    private sealed class NoopScope : IDisposable
    {
      public static NoopScope Instance { get; } = new();

      public void Dispose()
      {
      }
    }
  }
}
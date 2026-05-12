using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace ReaSharp;

public class ReaperConsoleLogger : ILogger
{
  public static void ClearLog()
  {
    var msgData = Marshal.StringToHGlobalAnsi(string.Empty);
    Reaper.ShowConsoleMsg.Invoke(msgData);
    Marshal.FreeHGlobal(msgData);
  }

  public static void WriteLog(string msg)
  {
    var msgData = Marshal.StringToHGlobalAnsi(msg + "\n");
    Reaper.ShowConsoleMsg.Invoke(msgData);
    Marshal.FreeHGlobal(msgData);
  }

  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
  {
    var prefix = logLevel switch
    {
      LogLevel.Trace => "[TRC]",
      LogLevel.Debug => "[DBG]",
      LogLevel.Warning => "[WRN]",
      LogLevel.Error => "[ERR]",
      LogLevel.Critical => "[CRT]",
      _ => "[INF]"
    };

    WriteLog(prefix + " " + formatter(state, exception));
  }

  public bool IsEnabled(LogLevel logLevel)
  {
    return true;
  }

  public IDisposable? BeginScope<TState>(TState state) where TState : notnull
  {
    return null;
  }
}
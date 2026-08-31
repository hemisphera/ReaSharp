using Microsoft.Extensions.Logging;

namespace Hsp.ReaSharp;

public sealed class ReaperConsoleLoggerProvider : ILoggerProvider
{
  public ILogger CreateLogger(string categoryName)
  {
    return new ReaperConsoleLogger();
  }

  public void Dispose()
  {
  }
}
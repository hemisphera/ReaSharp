using Microsoft.Extensions.Logging;

namespace ReaSharp;

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
using System.Runtime.InteropServices;

namespace ReaSharp;

public static class ReaperLogger
{
  public static void Log(string prefix, string text)
  {
    var msgData = Marshal.StringToHGlobalAnsi(prefix + "  " + text + "\n");
    Reaper.ShowConsoleMsg(msgData);
    Marshal.FreeHGlobal(msgData);
  }

  public static void ClearLog()
  {
    var msgData = Marshal.StringToHGlobalAnsi(string.Empty);
    Reaper.ShowConsoleMsg(msgData);
    Marshal.FreeHGlobal(msgData);
  }

  public static void LogInformation(string s)
  {
    Log("[INFO]", s);
  }

  public static void LogDebug(string s)
  {
    Log("[DEBG]", s);
  }

  public static void LogError(string msg)
  {
    Log("[FAIL]", msg);
  }
}
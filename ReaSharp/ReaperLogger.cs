using System.Runtime.InteropServices;

namespace ReaSharp;

public static class ReaperLogger
{
  public static void Log(string text)
  {
    var msgData = Marshal.StringToHGlobalAnsi(text + "\n");
    Reaper.ShowConsoleMsg(msgData);
    Marshal.FreeHGlobal(msgData);
  }
}
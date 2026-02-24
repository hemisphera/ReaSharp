using System.Runtime.InteropServices;

namespace ReaSharp;

[StructLayout(LayoutKind.Sequential)]
public struct ReaperPluginInfo
{
  public int CallerVersion;
  public IntPtr HwndMain;
  public IntPtr Register;
  public IntPtr GetFunc;
}
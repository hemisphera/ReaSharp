using System.Runtime.InteropServices;

namespace Hsp.ReaSharp;

[StructLayout(LayoutKind.Sequential)]
public struct ReaperPluginInfo
{
  public int CallerVersion;
  public nint HwndMain;
  public nint Register;
  public nint GetFunc;


  public static ReaperPluginInfo FromPointer(nint rec)
  {
    const int reaperPluginVersion = 0x20E;
    var info = Marshal.PtrToStructure<ReaperPluginInfo>(rec);
    if (info.GetFunc == nint.Zero || info.CallerVersion != reaperPluginVersion)
    {
      throw new Exception("Unable to initialize ReaperPlugininfo");
    }

    return info;
  }
}
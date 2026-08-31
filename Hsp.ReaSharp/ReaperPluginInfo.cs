using System.Runtime.InteropServices;

namespace Hsp.ReaSharp;

[StructLayout(LayoutKind.Sequential)]
public struct ReaperPluginInfo
{
  public int CallerVersion;
  public IntPtr HwndMain;
  public IntPtr Register;
  public IntPtr GetFunc;


  public static ReaperPluginInfo FromPointer(IntPtr rec)
  {
    const int reaperPluginVersion = 0x20E;
    var info = Marshal.PtrToStructure<ReaperPluginInfo>(rec);
    if (info.GetFunc == IntPtr.Zero || info.CallerVersion != reaperPluginVersion)
    {
      throw new Exception("Unable to initialize ReaperPlugininfo");
    }

    return info;
  }
}
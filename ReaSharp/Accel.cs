using System.Runtime.InteropServices;

namespace ReaSharp;

/// <summary>Maps to the Windows ACCEL struct used inside gaccel_register_t.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Accel
{
  public byte fVirt;   // virtual-key flags (0 for none)
  public ushort key;   // default hotkey (0 for none)
  public ushort cmd;   // filled in by REAPER with the assigned command ID
}
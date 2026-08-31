using System.Runtime.InteropServices;

namespace Hsp.ReaSharp;

/// <summary>Maps to REAPER's gaccel_register_t struct.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct GaccelRegister
{
  public Accel Accel;
  public IntPtr Desc; // const char* — ANSI description shown in the Action List
}
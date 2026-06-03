using System.Runtime.InteropServices;

namespace ReaSharp;

/// <summary>
/// Maps to REAPER's custom_action_register_t struct.
/// Use this instead of command_id+gaccel when registering actions that must be
/// triggerable via OSC /action/str, since it populates the named-command lookup table.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct CustomActionRegister
{
  public int UniqueSectionId; // 0/100=main/main alt, 32060=MIDI editor, etc.
  public IntPtr IdStr; // const char* — unique name across all sections
  public IntPtr Name; // const char* — display name in the Action List
  public IntPtr Extra; // reserved, must be IntPtr.Zero
}
using System;

namespace ReaSharp.Models;

/// <summary>
/// Represents the action context provided by REAPER's hookcommand2 callback.
/// </summary>
/// <remarks>
/// hookcommand2 signature: bool onAction(KbdSectionInfo *sec, int command, int val, int val2, int relmode, HWND hwnd)
/// val/val2 encode MIDI/OSC trigger data:
///   - MIDI CC:        val=[0..127], val2=-1
///   - MIDI pitch/OSC: val2 >= 0, OSC float = (val2 | (val &lt;&lt; 7)) / 16383.0
///   - relmode:        0=absolute, 1/2/3=relative adjust modes
/// </remarks>
public struct ActionContext
{
  /// <summary>Section unique ID (0=main, 100=main alt, 32060=MIDI editor, etc.).</summary>
  public int SectionId;

  /// <summary>For MIDI CC/OSC: value component [0..127].</summary>
  public int Val;

  /// <summary>For MIDI CC: -1. For MIDI pitch or OSC: &gt;=0.</summary>
  public int Val2;

  /// <summary>Relative mode: 0=absolute, 1/2/3=relative adjust.</summary>
  public int RelMode;

  /// <summary>HWND of the section window. Zero for MIDI/OSC triggers.</summary>
  public IntPtr Hwnd;

  public override string ToString()
  {
    return $"Val: {Val}, Val2: {Val2}, RelMode:{RelMode}";
  }
}
using System.Runtime.InteropServices;

namespace Hsp.ReaSharp.Models;

/// <summary>
/// Minimal marshalled representation of REAPER's KbdSectionInfo struct.
/// Only the first field (UniqueId) is needed from hookcommand2 callbacks.
/// </summary>
/// <remarks>
/// Full C SDK definition (reaper_plugin.h):
/// typedef struct _REAPER_KbdSectionInfo {
///   int uniqueID;            // 0=main, 100=main alt, 32060=MIDI editor, etc.
///   const char *name;
///   KbdCmd *action_list;
///   int action_list_cnt;
///   const KbdKeyBindingInfo *def_keys;
///   int def_keys_cnt;
///   bool (*onAction)(int cmd, int val, int valhw, int relmode, HWND hwnd);
///   void *accels;
///   void *recent_cmds;
///   void *extended_data[32];
/// } KbdSectionInfo;
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct KbdSectionInfo
{
  /// <summary>
  /// Unique section ID: 0=main section, 100=main alt, 32060=MIDI editor,
  /// 32061=MIDI event list editor, 32062=MIDI inline editor, 32063=media explorer.
  /// </summary>
  public int UniqueId;
}

using System.Diagnostics.CodeAnalysis;

namespace ReaSharp.Models;

using System.Collections.Generic;
using System.IO;

/// <summary>
/// Represents a single MIDI event in REAPER's GetAllEvts binary format.
/// </summary>
/// <remarks>
/// Wire layout per event (little-endian):
///   int32   offset      delta-ticks from the previous event
///   byte    flags       bit 0 = selected, bit 1 = muted
///   int32   msg_length  byte count of the following MIDI message
///   byte[]  data        raw MIDI message bytes (msg_length bytes)
/// The binary format uses delta-tick offsets, but the public API exposes
/// absolute tick positions. Conversion happens in ParseAll / SerializeAll.
/// </remarks>
public sealed class MediaItemMidiEvent
{
  /// <summary>Absolute tick position (PPQ-based) from the start of the MIDI item.</summary>
  public int Position { get; }

  /// <summary>True when the event is selected in REAPER.</summary>
  public bool IsSelected { get; }

  /// <summary>True when the event is muted in REAPER.</summary>
  public bool IsMuted { get; }

  /// <summary>
  /// Raw MIDI message bytes (e.g. status byte + data bytes).
  /// For notes this must always be a 3-byte message according to the MIDI specifications, where each
  /// part of the MIDI message is encoded into 3 bytes (status, channel, note velocity).
  /// </summary>
  public byte[] Data { get; }


  public MediaItemMidiEvent(int position, bool isSelected, bool isMuted, byte[] data)
  {
    Position = position;
    IsSelected = isSelected;
    IsMuted = isMuted;
    Data = (byte[])data.Clone();
  }

  public static bool TryDeserialize(byte[] buffer, [NotNullWhen(true)] out MediaItemMidiEvent[]? events)
  {
    events = null;
    try
    {
      events = Deserialize(buffer);
      return true;
    }
    catch
    {
      return false;
    }
  }

  /// <summary>Parses the full byte buffer returned by GetAllEvts into individual events.</summary>
  public static MediaItemMidiEvent[] Deserialize(byte[] buffer)
  {
    var events = new List<MediaItemMidiEvent>();
    using var ms = new MemoryStream(buffer, writable: false);
    using var reader = new BinaryReader(ms);

    var absoluteTick = 0;
    while (ms.Position < ms.Length)
    {
      var offset = reader.ReadInt32();
      var flags = reader.ReadByte();
      var msgLen = reader.ReadInt32();
      var data = reader.ReadBytes(msgLen);

      absoluteTick += offset;
      events.Add(new MediaItemMidiEvent(
        absoluteTick,
        isSelected: (flags & 0x01) != 0,
        isMuted: (flags & 0x02) != 0,
        data));
    }

    return events.ToArray();
  }

  /// <summary>Serializes a sequence of events back into the GetAllEvts wire format.</summary>
  public static byte[] Serialize(IEnumerable<MediaItemMidiEvent> events)
  {
    using var ms = new MemoryStream();
    using var writer = new BinaryWriter(ms);

    var previousTick = 0;
    foreach (var evt in events.OrderBy(e => e.Position))
    {
      writer.Write(evt.Position - previousTick);
      previousTick = evt.Position;

      byte flags = 0;
      if (evt.IsSelected) flags |= 0x01;
      if (evt.IsMuted) flags |= 0x02;
      writer.Write(flags);

      writer.Write(evt.Data.Length);
      writer.Write(evt.Data);
    }

    return ms.ToArray();
  }
}
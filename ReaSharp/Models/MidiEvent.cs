namespace ReaSharp.Models;

public sealed class MidiEvent
{
  public int Sequence { get; set; }
  public int DeviceIndex { get; set; }
  public int BufferSize { get; set; }
  public byte Status { get; set; }
  public byte Data1 { get; set; }
  public byte Data2 { get; set; }

  public byte Channel => (byte)(Status & 0x0F);
  public byte Message => (byte)(Status >> 4);

  public override string ToString()
  {
    return $"{Sequence}: {Channel} {Message} {Data1} {Data2}";
  }
}
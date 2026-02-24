namespace ReaSharp;

public sealed class MidiEvent
{
  public int Sequence { get; set; }
  public int DeviceIndex { get; set; }
  public int BufferSize { get; set; }
  public byte Status { get; set; }
  public byte Data1 { get; set; }
  public byte Data2 { get; set; }

  public override string ToString()
  {
    return $"{Sequence}: {Status} {Data1} {Data2}";
  }
}
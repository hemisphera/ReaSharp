namespace Hsp.ReaSharp.Models;

public sealed class MidiEvent
{
  public int Sequence { get; set; }

  public int DeviceIndex { get; set; }

  public int BufferSize => Buffer.Length;

  public byte[] Buffer { get; }

  public byte Status
  {
    get => (byte)(Buffer.Length > 0 ? Buffer[0] : 0);
    set
    {
      if (Buffer.Length > 0)
        Buffer[0] = value;
    }
  }

  public byte Data1
  {
    get => (byte)(Buffer.Length > 1 ? Buffer[1] : 0);
    set
    {
      if (Buffer.Length > 1)
        Buffer[1] = value;
    }
  }

  public byte Data2
  {
    get => (byte)(Buffer.Length > 2 ? Buffer[2] : 0);
    set
    {
      if (Buffer.Length > 2)
        Buffer[2] = value;
    }
  }

  public byte Channel
  {
    get => (byte)(Status & 0x0F);
    set => Status = (byte)((Status & 0xF0) | (value & 0x0F));
  }

  public byte MessageType
  {
    get => (byte)(Status >> 4);
    set => Status = (byte)((Status & 0x0F) | ((value & 0x0F) << 4));
  }


  public MidiEvent(int bufferSize)
    : this(new byte[bufferSize])
  {
  }

  public MidiEvent(params byte[] buffer)
  {
    Buffer = buffer;
  }


  public override string ToString()
  {
    return $"{Sequence}: {Channel} {MessageType} {Data1} {Data2}";
  }
}
namespace Hsp.ReaSharp;

public interface IGmemService
{
  string? ConnectedName { get; }
  bool IsConnected { get; }

  void Connect(string name, bool isAlloc = true);
  double Read(int index);
  void Write(int index, double value);

  public int ReadInt32(int index)
  {
    return (int)Math.Round(Read(index), MidpointRounding.AwayFromZero);
  }

  public void WriteInt32(int index, int value)
  {
    Write(index, value);
  }

  public bool ReadBoolean(int index)
  {
    return Math.Abs(Read(index)) > double.Epsilon;
  }

  public void WriteBoolean(int index, bool value)
  {
    Write(index, value ? 1.0 : 0.0);
  }

  public TEnum ReadEnum<TEnum>(int index) where TEnum : struct, Enum
  {
    var raw = ReadInt32(index);
    if (!Enum.IsDefined(typeof(TEnum), raw))
      throw new Exception($"Value {raw} at slot {index} is not a valid {typeof(TEnum).Name}.");
    return (TEnum)Enum.ToObject(typeof(TEnum), raw);
  }

  public void WriteEnum<TEnum>(int index, TEnum value) where TEnum : struct, Enum
  {
    WriteInt32(index, Convert.ToInt32(value));
  }

  public int Increment(int index, int value = 1)
  {
    var current = ReadInt32(index);
    var newValue = current + value;
    WriteInt32(index, newValue);
    return newValue;
  }
}
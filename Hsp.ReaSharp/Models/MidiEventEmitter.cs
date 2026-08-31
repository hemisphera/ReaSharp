namespace Hsp.ReaSharp.Models;

public class MidiEventEmitter
{
  public static readonly MidiEventEmitter VirtualKeyboard = new(0);
  public static readonly MidiEventEmitter Control = new(1);


  private readonly int _mode;


  public MidiEventEmitter(MidiDevice device)
    : this(device.Id + 16)
  {
  }

  private MidiEventEmitter(int mode)
  {
    _mode = mode;
  }


  public MidiEventEmitter Send(MidiEvent evt)
  {
    Reaper.StuffMIDIMessage.Invoke(_mode, evt.Status, evt.Data1, evt.Data2);
    return this;
  }
}
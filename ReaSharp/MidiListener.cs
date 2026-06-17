using ReaSharp.Models;

namespace ReaSharp;

public sealed class MidiListener : IDisposable
{
  private readonly CancellationTokenSource _cts;

  public event EventHandler<MidiEvent>? MidiReceived;

  public TimeSpan PollFrequency { get; set; } = TimeSpan.FromMilliseconds(50);


  public MidiListener()
  {
    _cts = new CancellationTokenSource();
    var token = _cts.Token;
    Task.Run(async () =>
    {
      int? seq = null;
      while (!token.IsCancellationRequested)
      {
        var events = ReaperGlobal.GetRecentMidiEvents(seq);
        foreach (var midiEvent in events)
        {
          MidiReceived?.Invoke(this, midiEvent);
          if (seq == null || seq < midiEvent.Sequence)
          {
            seq = midiEvent.Sequence;
          }
        }

        await Task.Delay(PollFrequency, token);
      }
    });
  }

  public void Dispose()
  {
    _cts.Cancel();
    _cts.Dispose();
  }
}
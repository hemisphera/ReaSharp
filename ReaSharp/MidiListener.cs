using ReaSharp.Models;

namespace ReaSharp;

public sealed class MidiListener : IDisposable
{
  private readonly CancellationTokenSource _cts;

  public Action<MidiEvent> MidiCallback { get; }

  public TimeSpan PollFrequency { get; set; } = TimeSpan.FromSeconds(1);


  public MidiListener(Action<MidiEvent> midiCallback)
  {
    MidiCallback = midiCallback;
    _cts = new CancellationTokenSource();
    var token = _cts.Token;
    Task.Run(async () =>
    {
      int? seq = null;
      while (!token.IsCancellationRequested)
      {
        await Task.Delay(PollFrequency, token);

        var events = ReaperApi.GetRecentMidiEvents(seq);
        foreach (var midiEvent in events)
        {
          MidiCallback.Invoke(midiEvent);
          if (seq == null || seq < midiEvent.Sequence)
          {
            seq = midiEvent.Sequence;
          }
        }
      }
    });
  }

  public void Dispose()
  {
    _cts.Cancel();
    _cts.Dispose();
  }
}
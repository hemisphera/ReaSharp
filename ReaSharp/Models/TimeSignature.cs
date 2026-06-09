namespace ReaSharp.Models;

public class TimeSignature
{
  public Transport Transport { get; }
  public TimeSpan Time { get; private set; }
  public double Beats { get; private set; }
  public int Measures { get; private set; }
  public int Numerator { get; private set; }
  public int Denominator { get; private set; }
  public double Tempo { get; private set; }


  private TimeSignature(Transport transport)
  {
    Transport = transport;
  }


  public static TimeSignature Get(Transport transport, TimeSpan? time = null)
  {
    var result = new TimeSignature(transport);
    result.Update(time);
    return result;
  }

  public void Update(TimeSpan? time = null)
  {
    time ??= Transport.PlayheadOrCursorPosition;
    double tempoOut;
    int numOut, denomOut;
    unsafe
    {
      double timeOut;
      Reaper.TimeMap_GetTimeSigAtTime.Invoke(
        Transport.Project.ReaperHandle,
        (nint)(&timeOut),
        (nint)(&numOut),
        (nint)(&denomOut),
        (nint)(&tempoOut));
    }

    Time = time.Value;
    Denominator = denomOut;
    Numerator = numOut;
    Tempo = tempoOut;
    int measuresOut;
    unsafe
    {
      Beats = Reaper.TimeMap2_timeToBeats.Invoke(
        Transport.Project.ReaperHandle,
        time.Value.TotalSeconds,
        (nint)(&measuresOut),
        nint.Zero, nint.Zero, nint.Zero);
    }

    Measures = measuresOut;
  }
}
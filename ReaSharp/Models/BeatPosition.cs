namespace ReaSharp.Models;

public struct BeatPosition : IComparable<BeatPosition>, IEquatable<BeatPosition>
{
  public int Measure { get; set; }
  public double Beats { get; set; }
  public int MeasureLength { get; init; }
  public double TotalBeats { get; init; }
  public int Denominator { get; init; }

  public TimeSpan ToTime(Project? project = null)
  {
    unsafe
    {
      project ??= Project.Default;
      var measure = Measure;
      var time = Reaper.TimeMap2_beatsToTime.Invoke(project.ReaperHandle, Beats, (IntPtr)(&measure));
      return TimeSpan.FromSeconds(time);
    }
  }

  public static BeatPosition FromTime(TimeSpan time, Project? project = null)
  {
    unsafe
    {
      project ??= Project.Default;
      var measure = 0;
      var measureLength = 0;
      double totalBeats = 0;
      var denominator = 0;
      var beats = Reaper.TimeMap2_timeToBeats.Invoke(
        project.ReaperHandle, time.TotalSeconds,
        (IntPtr)(&measure), (IntPtr)(&measureLength), (IntPtr)(&totalBeats), (IntPtr)(&denominator)
      );
      beats = Math.Round(beats, 5);
      totalBeats = Math.Round(totalBeats, 5);

      // acount for rounding errors where beats is very close to measureLength, but not quite due to floating point precision issues
      if (Math.Abs(beats - measureLength) <= 0.00001)
      {
        measure++;
        beats = 0;
      }

      return new BeatPosition
      {
        Measure = measure,
        Beats = beats,
        TotalBeats = totalBeats,
        MeasureLength = measureLength,
        Denominator = denominator
      };
    }
  }

  public int CompareTo(BeatPosition other)
  {
    return Measure != other.Measure ? Measure.CompareTo(other.Measure) : Beats.CompareTo(other.Beats);
  }

  public bool Equals(BeatPosition other)
  {
    return Measure == other.Measure && Math.Abs(Beats - other.Beats) < 0.0001;
  }

  public override string ToString()
  {
    return $"{Measure}:{Beats} ({TotalBeats}) {MeasureLength}:{Denominator}";
  }
}
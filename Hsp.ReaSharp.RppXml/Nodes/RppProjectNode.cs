namespace Hsp.ReaSharp.RppXml.Nodes;

/// <summary>
/// Typed RPPXML node for a REAPER project (REAPER_PROJECT / PROJECT).
/// Common properties are exposed as typed class members; all other properties
/// remain accessible via the inherited generic <see cref="RppNode"/> API.
/// </summary>
public sealed class RppProjectNode : RppNode
{
  public RppProjectNode() : base("REAPER_PROJECT")
  {
  }

  /// <summary>Project tempo in BPM (BPM[0]).</summary>
  public double Bpm
  {
    get => GetPropertyValue("BPM", 0)?.AsDouble() ?? 120.0;
    set => SetPropertyValue("BPM", 0, RppValue.From(value));
  }

  /// <summary>Project sample rate in Hz (SRATE[0]).</summary>
  public int SampleRate
  {
    get => GetPropertyValue("SRATE", 0)?.AsInt32() ?? 44100;
    set => SetPropertyValue("SRATE", 0, RppValue.From(value));
  }

  /// <summary>Time signature numerator (TEMPO[1] — beats per measure).</summary>
  public int TimeSigNumerator
  {
    get => GetPropertyValue("TEMPO", 1)?.AsInt32() ?? 4;
    set => SetPropertyValue("TEMPO", 1, RppValue.From(value));
  }

  /// <summary>Time signature denominator (TEMPO[2]).</summary>
  public int TimeSigDenominator
  {
    get => GetPropertyValue("TEMPO", 2)?.AsInt32() ?? 4;
    set => SetPropertyValue("TEMPO", 2, RppValue.From(value));
  }

  /// <summary>All direct TRACK children in the project.</summary>
  public IEnumerable<RppTrackNode> Tracks
  {
    get { return FindChildren("TRACK").OfType<RppTrackNode>(); }
  }
}
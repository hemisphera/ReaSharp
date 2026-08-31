namespace Hsp.ReaSharp.RppXml.Nodes;

/// <summary>
/// Typed RPPXML node for a REAPER track (TRACK).
/// Common properties are exposed as typed class members; all other properties
/// remain accessible via the inherited generic <see cref="RppNode"/> API.
/// </summary>
public sealed class RppTrackNode : RppNode
{
  public RppTrackNode() : base("TRACK") { }

  // ── Typed properties ───────────────────────────────────────────────────

  /// <summary>Track name (NAME[0]).</summary>
  public string? TrackName
  {
    get => GetPropertyValue("NAME", 0)?.AsString();
    set { if (value is not null) SetPropertyValue("NAME", 0, RppValue.From(value)); }
  }

  /// <summary>Mute flag (MUTESOLO[0]).</summary>
  public bool Mute
  {
    get => GetPropertyValue("MUTESOLO", 0)?.AsBool() ?? false;
    set => SetPropertyValue("MUTESOLO", 0, RppValue.From(value));
  }

  /// <summary>Solo state (MUTESOLO[1]).</summary>
  public int Solo
  {
    get => GetPropertyValue("MUTESOLO", 1)?.AsInt32() ?? 0;
    set => SetPropertyValue("MUTESOLO", 1, RppValue.From(value));
  }

  /// <summary>Volume as a linear multiplier (VOLPAN[0]). 1.0 = unity gain.</summary>
  public double Volume
  {
    get => GetPropertyValue("VOLPAN", 0)?.AsDouble() ?? 1.0;
    set => SetPropertyValue("VOLPAN", 0, RppValue.From(value));
  }

  /// <summary>Pan position, –1.0 (full left) to 1.0 (full right) (VOLPAN[1]).</summary>
  public double Pan
  {
    get => GetPropertyValue("VOLPAN", 1)?.AsDouble() ?? 0.0;
    set => SetPropertyValue("VOLPAN", 1, RppValue.From(value));
  }

  /// <summary>
  /// Packed RGB colour integer (COLOR[0]).
  /// 0 means the track uses the default colour.
  /// </summary>
  public int Color
  {
    get => GetPropertyValue("COLOR", 0)?.AsInt32() ?? 0;
    set => SetPropertyValue("COLOR", 0, RppValue.From(value));
  }

  /// <summary>
  /// Folder/bus state (ISBUS[0]).
  /// 0 = normal track, 1 = folder start, –1 = last track in folder.
  /// </summary>
  public int IsBus
  {
    get => GetPropertyValue("ISBUS", 0)?.AsInt32() ?? 0;
    set => SetPropertyValue("ISBUS", 0, RppValue.From(value));
  }

  // ── Child convenience ──────────────────────────────────────────────────

  /// <summary>All ITEM child nodes on this track.</summary>
  public IEnumerable<RppNode> Items
  {
    get { return FindChildren("ITEM"); }
  }
}

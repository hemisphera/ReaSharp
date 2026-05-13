namespace ReaSharp.RppXml;

/// <summary>
/// Central configuration for RPPXML parsing and serialisation behaviour.
/// Extend the members below as new format patterns are discovered.
/// </summary>
public static class RppSchema
{
  // ── Implicit child-node starters (spec point 11) ───────────────────────
  //
  // Maps parent node names (case-insensitive) to the set of property names
  // (case-insensitive) that implicitly start a new child node inside that
  // parent, rather than being plain properties of the parent.
  //
  // Add or remove entries here as additional patterns are discovered.
  // Example: inside FXCHAIN every FX begins with "BYPASS" instead of "<NAME".

  public static readonly Dictionary<string, HashSet<string>> ImplicitChildNodeStarters =
    new(StringComparer.OrdinalIgnoreCase)
    {
      ["FXCHAIN"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BYPASS" }
    };

  // ── Node factory ───────────────────────────────────────────────────────
  //
  // Replace or wrap this delegate to return custom RppNode subclasses for
  // specific node names without changing the reader.

  public static Func<string, RppNode> NodeFactory { get; set; } = DefaultFactory;

  private static RppNode DefaultFactory(string name) => name.ToUpperInvariant() switch
  {
    "REAPER_PROJECT" or "PROJECT" => new Nodes.RppProjectNode(),
    "TRACK" => new Nodes.RppTrackNode(),
    "NOTES" => new Nodes.RppNotesNode(),
    _ => new RppNode(name)
  };
}
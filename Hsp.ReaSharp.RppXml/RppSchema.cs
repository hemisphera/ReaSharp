namespace Hsp.ReaSharp.RppXml;

/// <summary>
/// Central configuration for RPPXML parsing and serialisation behaviour.
/// Extend the members below as new format patterns are discovered.
/// </summary>
public static class RppSchema
{
  // ── Implicit child-node starters (spec point 11) ───────────────────────
  //
  // Maps parent node names (case-insensitive) to an inner dictionary that
  // maps each starter property name (case-insensitive) to the logical node
  // name that should be used for the implicitly created child node.
  // When the mapped node name is null or empty the starter property name is
  // used as the node name instead.
  //
  // Add or remove entries here as additional patterns are discovered.
  // Example: inside FXCHAIN every FX begins with property "BYPASS" and the
  // implicit child node should be called "FX".

  public static readonly Dictionary<string, Dictionary<string, string>> ImplicitChildNodeStarters =
    new(StringComparer.OrdinalIgnoreCase)
    {
      ["FXCHAIN"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
      {
        ["BYPASS"] = "FX"
      }
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
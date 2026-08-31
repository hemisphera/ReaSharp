namespace Hsp.ReaSharp.RppXml;

/// <summary>
/// A block of pipe-prefixed multi-line text inside an RPPXML node.
/// In the file each line is written as <c>| &lt;text&gt;</c>.
/// </summary>
public sealed class RppMultilineText : IRppEntry
{
  public List<string> Lines { get; } = [];

  /// <summary>All lines joined with <see cref="Environment.NewLine"/>.</summary>
  public string Text => string.Join(Environment.NewLine, Lines);

  public RppMultilineText(IEnumerable<string>? lines = null)
  {
    if (lines != null)
    {
      Lines.AddRange(lines);
    }
  }
}
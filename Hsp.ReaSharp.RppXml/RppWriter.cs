using System.Text;

namespace Hsp.ReaSharp.RppXml;

/// <summary>Serialises an <see cref="RppNode"/> tree back to RPPXML text.</summary>
public sealed class RppWriter
{
  private readonly TextWriter _out;
  private readonly string _indentUnit;
  private int _depth;

  public RppWriter(TextWriter output, string indentUnit = "  ")
  {
    _out = output;
    _indentUnit = indentUnit;
  }

  // ── Public entry points ────────────────────────────────────────────────

  /// <summary>Serialises <paramref name="root"/> to a string with CRLF line endings.</summary>
  public static string WriteToString(RppNode root, string indentUnit = "  ")
  {
    var sb = new StringBuilder();
    using var sw = new StringWriter(sb) { NewLine = "\r\n" };
    new RppWriter(sw, indentUnit).WriteNode(root);
    return sb.ToString();
  }

  /// <summary>Writes <paramref name="root"/> to <paramref name="path"/> with CRLF line endings.</summary>
  public static void WriteToFile(RppNode root, string path, string indentUnit = "  ")
  {
    using var sw = new StreamWriter(path, false, Encoding.UTF8) { NewLine = "\r\n" };
    new RppWriter(sw, indentUnit).WriteNode(root);
  }

  /// <summary>Writes <paramref name="root"/> to the configured <see cref="TextWriter"/>.</summary>
  public void WriteNode(RppNode node)
  {
    if (!node.IsImplicit)
    {
      // Opening tag: <NAME [default values...]
      var header = node.DefaultValues.Count > 0
        ? $"<{node.Name} {string.Join(' ', node.DefaultValues)}"
        : $"<{node.Name}";
      WriteLine(header);
      _depth++;
    }

    foreach (var entry in node.Entries)
    {
      switch (entry)
      {
        case RppProperty prop:
          WriteLine(prop.ToString());
          break;

        case RppMultilineText ml:
          foreach (var textLine in ml.Lines)
            WriteLine($"| {textLine}");
          break;

        case RppNode child:
          WriteNode(child);
          break;
      }
    }

    if (!node.IsImplicit)
    {
      _depth--;
      WriteLine(">");
    }
  }

  // ── Helpers ────────────────────────────────────────────────────────────

  private void WriteLine(string text)
  {
    if (_depth > 0)
      _out.Write(string.Concat(Enumerable.Repeat(_indentUnit, _depth)));
    _out.WriteLine(text);
  }
}

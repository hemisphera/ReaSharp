using System.Diagnostics.CodeAnalysis;

namespace Hsp.ReaSharp.RppXml;

/// <summary>Parses an RPPXML text stream into an <see cref="RppNode"/> tree.</summary>
public sealed class RppReader
{
  public static bool TryRead(string content, [NotNullWhen(true)] out RppNode? node)
  {
    node = null;
    try
    {
      node = Read(content);
      return true;
    }
    catch
    {
      return false;
    }
  }

  /// <summary>Parses RPPXML from a string.</summary>
  public static RppNode Read(string content)
  {
    using var sr = new StringReader(content);
    return Read(sr);
  }

  /// <summary>Parses RPPXML from any <see cref="TextReader"/>.</summary>
  public static RppNode Read(TextReader reader)
  {
    var stack = new Stack<RppNode>();
    RppNode? root = null;

    while (reader.ReadLine() is { } line)
    {
      var trimmed = line.TrimStart();
      if (trimmed.Length == 0) continue;

      // ── Node open: <NAME [header values...] ───────────────────────────
      if (trimmed[0] == '<')
      {
        var (name, headerValues) = ParseNodeHeader(trimmed[1..]);
        var node = RppSchema.NodeFactory(name);
        node.Name = name;
        node.DefaultValues.AddRange(headerValues);

        if (stack.Count > 0)
        {
          var parent = stack.Peek();
          node.Parent = parent;
          parent.Entries.Add(node);
        }
        else
        {
          root = node;
        }

        stack.Push(node);
        continue;
      }

      // ── Node close: > ─────────────────────────────────────────────────
      if (trimmed == ">")
      {
        // Pop any open implicit node before closing the real parent
        if (stack.Count > 0 && stack.Peek().IsImplicit)
          stack.Pop();

        if (stack.Count > 0)
          stack.Pop();

        continue;
      }

      // ── Multi-line text: | <text> ─────────────────────────────────────
      if (trimmed[0] == '|')
      {
        if (stack.Count == 0) continue;

        var text = trimmed.Length > 1 && trimmed[1] == ' '
          ? trimmed[2..]
          : trimmed[1..];

        var current = stack.Peek();
        if (current.Entries.Count > 0 && current.Entries[^1] is RppMultilineText existing)
          existing.Lines.Add(text);
        else
        {
          var block = new RppMultilineText();
          block.Lines.Add(text);
          current.Entries.Add(block);
        }

        continue;
      }

      // ── Property line ─────────────────────────────────────────────────
      if (stack.Count == 0) continue;

      var tokens = Tokenize(trimmed);
      if (tokens.Count == 0) continue;

      var propName = tokens[0];
      var propValues = tokens.Skip(1).Select(RppValue.FromRaw).ToList();
      var top = stack.Peek();

      // Determine the enclosing non-implicit node to check implicit starters
      var enclosingName = top.IsImplicit && stack.Count >= 2
        ? PeekAt(stack, 1).Name
        : top.Name;

      if (RppSchema.ImplicitChildNodeStarters.TryGetValue(enclosingName, out var starters)
          && starters.TryGetValue(propName, out var implicitNodeName))
      {
        // Close previous implicit sibling (if any), then open a new one
        if (top.IsImplicit)
        {
          stack.Pop();
          top = stack.Peek();
        }

        // Use the configured node name; fall back to the starter property name.
        var effectiveName = string.IsNullOrEmpty(implicitNodeName) ? propName : implicitNodeName;
        var implicitNode = RppSchema.NodeFactory(effectiveName);
        implicitNode.Name = effectiveName;
        implicitNode.IsImplicit = true;
        implicitNode.Parent = top;
        implicitNode.Entries.Add(new RppProperty(propName, propValues));
        top.Entries.Add(implicitNode);
        stack.Push(implicitNode);
      }
      else
      {
        top.Entries.Add(new RppProperty(propName, propValues));
      }
    }

    return root ?? throw new InvalidOperationException("No root node found in RPPXML input.");
  }

  private static (string name, List<RppValue> headerValues) ParseNodeHeader(string text)
  {
    var tokens = Tokenize(text);
    return tokens.Count == 0 ? (string.Empty, []) : (tokens[0], tokens.Skip(1).Select(RppValue.FromRaw).ToList());
  }

  /// <summary>
  /// Tokenises a single line, treating quoted strings as atomic tokens.
  /// Raw tokens are returned exactly as they appear (quotes included).
  /// </summary>
  public static List<string> Tokenize(string line, bool unquote = false)
  {
    var tokens = new List<string>();
    var i = 0;

    while (i < line.Length)
    {
      // Skip whitespace between tokens
      while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
        i++;

      if (i >= line.Length) break;

      if (line[i] == '"')
      {
        // Quoted string: scan to the closing unescaped quote
        var start = i++;
        while (i < line.Length)
        {
          if (line[i] == '\\' && i + 1 < line.Length && line[i + 1] == '"')
            i += 2; // skip escaped quote
          else if (line[i] == '"')
          {
            i++; // consume closing quote
            break;
          }
          else
            i++;
        }

        tokens.Add(line[start..i]);
      }
      else
      {
        // Unquoted token: read until whitespace
        var start = i;
        while (i < line.Length && line[i] != ' ' && line[i] != '\t')
          i++;
        tokens.Add(line[start..i]);
      }
    }

    return unquote
      ? tokens.Select(t => t.StartsWith('"') && t.EndsWith('"') ? t.Substring(1, t.Length - 2) : t).ToList()
      : tokens;
  }

  private static RppNode PeekAt(Stack<RppNode> stack, int depth)
  {
    return stack.ToArray()[depth];
  }
}
using System.Collections;

namespace Hsp.ReaSharp.Utils;

/// <summary>
/// A simple INI file parser that reads key/value pairs organized by section.
/// </summary>
public sealed class IniFile : IEnumerable<IniSection>
{
  private readonly List<IniSection> _sections = [];

  public IReadOnlyList<IniSection> Sections => _sections;

  public IniSection? this[string name] =>
    _sections.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

  public static IniFile Load(string filePath)
  {
    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    return Load(stream);
  }

  public static IniFile Load(Stream stream)
  {
    var file = new IniFile();
    IniSection? current = null;

    using var reader = new StreamReader(stream);
    while (reader.ReadLine() is { } line)
    {
      line = line.Trim();

      if (line.Length == 0 || line[0] == ';' || line[0] == '#')
        continue;

      if (line[0] == '[' && line[^1] == ']')
      {
        var name = line[1..^1].Trim();
        current = new IniSection(name);
        file._sections.Add(current);
        continue;
      }

      var eq = line.IndexOf('=');
      if (eq <= 0 || current is null)
        continue;

      var key = line[..eq].Trim();
      var value = line[(eq + 1)..].Trim();
      current.Set(key, value);
    }

    return file;
  }

  public IEnumerator<IniSection> GetEnumerator() => _sections.GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
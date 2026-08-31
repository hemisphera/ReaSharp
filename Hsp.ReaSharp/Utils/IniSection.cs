using System.Collections;

namespace Hsp.ReaSharp.Utils;

public sealed class IniSection : IEnumerable<KeyValuePair<string, string>>
{
  private readonly Dictionary<string, string> _entries = new(StringComparer.OrdinalIgnoreCase);

  public string Name { get; }

  public string? this[string key] => _entries.GetValueOrDefault(key);

  internal IniSection(string name) => Name = name;

  internal void Set(string key, string value) => _entries[key] = value;

  public bool TryGetInt(string key, out int value)
  {
    value = 0;
    if (!TryGet(key, out var str)) return false;
    return int.TryParse(str, out value);
  }

  public bool TryGet(string key, out string value)
  {
    if (_entries.TryGetValue(key, out var v))
    {
      value = v;
      return true;
    }

    value = string.Empty;
    return false;
  }

  public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
  {
    return _entries.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
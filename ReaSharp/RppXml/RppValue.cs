using System.Globalization;

namespace ReaSharp.RppXml;

/// <summary>
/// A single value token within an RPPXML property or node default values.
/// Wraps the raw token and provides typed accessors.
/// </summary>
public sealed class RppValue
{
  private readonly string _raw;

  private RppValue(string raw)
  {
    _raw = raw;
  }

  /// <summary>Creates a value from a raw token exactly as it appears in the file.</summary>
  public static RppValue FromRaw(string raw)
  {
    return new RppValue(raw);
  }

  /// <summary>Creates a string value, quoting it if it contains whitespace or quotes.</summary>
  public static RppValue From(string value)
  {
    return new RppValue(NeedsQuoting(value) ? $"\"{EscapeQuotes(value)}\"" : value);
  }

  public static RppValue From(int value)
  {
    return new RppValue(value.ToString(CultureInfo.InvariantCulture));
  }

  public static RppValue From(long value)
  {
    return new RppValue(value.ToString(CultureInfo.InvariantCulture));
  }

  public static RppValue From(double value)
  {
    return new RppValue(value.ToString(CultureInfo.InvariantCulture));
  }

  public static RppValue From(float value)
  {
    return new RppValue(value.ToString(CultureInfo.InvariantCulture));
  }

  public static RppValue From(bool value)
  {
    return new RppValue(value ? "1" : "0");
  }

  public string Raw => _raw;

  public string AsString()
  {
    if (_raw is ['"', _, ..] && _raw[^1] == '"')
    {
      return _raw[1..^1].Replace("\\\"", "\"");
    }

    return _raw;
  }

  public int AsInt32()
  {
    return int.Parse(_raw, CultureInfo.InvariantCulture);
  }

  public long AsInt64()
  {
    return long.Parse(_raw, CultureInfo.InvariantCulture);
  }

  public double AsDouble()
  {
    return double.Parse(_raw, CultureInfo.InvariantCulture);
  }

  public float AsSingle()
  {
    return float.Parse(_raw, CultureInfo.InvariantCulture);
  }

  public bool AsBool()
  {
    return _raw == "1";
  }

  public override string ToString()
  {
    return _raw;
  }


  private static bool NeedsQuoting(string v)
  {
    return v.Length == 0 || v.Any(c => c is ' ' or '\t' or '"');
  }

  private static string EscapeQuotes(string v)
  {
    return v.Replace("\"", "\\\"");
  }
}
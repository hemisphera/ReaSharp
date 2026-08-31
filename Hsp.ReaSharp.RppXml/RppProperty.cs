namespace Hsp.ReaSharp.RppXml;

/// <summary>
/// A property line inside an RPPXML node: a name followed by zero or more typed values.
/// </summary>
public sealed class RppProperty : IRppEntry
{
  public string Name { get; set; }

  public List<RppValue> Values { get; }

  public RppValue? this[int index] => index >= 0 && index < Values.Count ? Values[index] : null;


  public RppProperty(string name, IEnumerable<RppValue>? values = null)
  {
    Name = name;
    Values = values?.ToList() ?? [];
  }


  public override string ToString()
  {
    return Values.Count > 0 ? $"{Name} {string.Join(' ', Values)}" : Name;
  }
}
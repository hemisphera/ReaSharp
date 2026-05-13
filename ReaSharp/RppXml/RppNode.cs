namespace ReaSharp.RppXml;

/// <summary>
/// A generic RPPXML node. Preserves all data (properties, multi-line text, child nodes)
/// to guarantee lossless round-trips for unknown node types.
/// </summary>
public class RppNode : IRppEntry
{
  /// <summary>Node name as it appears after the opening <c>&lt;</c> (e.g. "TRACK").</summary>
  public string Name { get; set; }

  /// <summary>
  /// Values that appear on the opening <c>&lt;NAME val1 val2 ...</c> line,
  /// directly after the node name. Accessible by index via
  /// <see cref="GetDefaultValue"/> and <see cref="SetDefaultValue"/>.
  /// </summary>
  public List<RppValue> DefaultValues { get; } = [];

  /// <summary>
  /// Ordered list of entries inside this node: <see cref="RppProperty"/>,
  /// <see cref="RppMultilineText"/>, or nested <see cref="RppNode"/> children.
  /// </summary>
  public List<IRppEntry> Entries { get; } = [];

  /// <summary>
  /// When <see langword="true"/> this node was opened implicitly (no <c>&lt;NAME</c> header
  /// line) as defined by <see cref="RppSchema.ImplicitChildNodeStarters"/>.
  /// </summary>
  public bool IsImplicit { get; internal set; }


  public RppNode(string? name = null)
  {
    Name = name ?? string.Empty;
  }


  public IEnumerable<RppNode> Children => Entries.OfType<RppNode>();

  public IEnumerable<RppProperty> Properties => Entries.OfType<RppProperty>();

  /// <summary>Value at <paramref name="index"/> of the node's default values, or null.</summary>
  public RppValue? GetDefaultValue(int index)
  {
    return index >= 0 && index < DefaultValues.Count ? DefaultValues[index] : null;
  }

  /// <summary>
  /// Sets the value at <paramref name="index"/> of the node's default values.
  /// Pads with empty values when index exceeds current length.
  /// </summary>
  public void SetDefaultValue(int index, RppValue value)
  {
    while (DefaultValues.Count <= index)
    {
      DefaultValues.Add(RppValue.From(string.Empty));
    }

    DefaultValues[index] = value;
  }

  /// <summary>First direct child with <paramref name="name"/> (case-insensitive), or null.</summary>
  public RppNode? FindChild(string name)
  {
    return Children.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>First direct child with <paramref name="name"/> (case-insensitive), or null.</summary>
  public T? FindChild<T>() where T : RppNode
  {
    return Children.OfType<T>().FirstOrDefault();
  }

  /// <summary>All direct children with <paramref name="name"/> (case-insensitive).</summary>
  public IEnumerable<RppNode> FindChildren(string name)
  {
    return Children.Where(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>Depth-first enumeration of all descendant nodes.</summary>
  public IEnumerable<RppNode> Descendants()
  {
    foreach (var child in Children)
    {
      yield return child;
      foreach (var d in child.Descendants())
      {
        yield return d;
      }
    }
  }

  /// <summary>This node plus all descendants (depth-first).</summary>
  public IEnumerable<RppNode> DescendantsAndSelf()
  {
    yield return this;
    foreach (var d in Descendants())
    {
      yield return d;
    }
  }

  /// <summary>First node anywhere in the subtree with <paramref name="name"/>, or null.</summary>
  public RppNode? Find(string name)
  {
    return DescendantsAndSelf()
      .FirstOrDefault(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>All nodes anywhere in the subtree with <paramref name="name"/>.</summary>
  public IEnumerable<RppNode> FindAll(string name)
  {
    return DescendantsAndSelf()
      .Where(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>First property with <paramref name="name"/> (case-insensitive), or null.</summary>
  public RppProperty? GetProperty(string name)
  {
    return Properties.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>Value at <paramref name="index"/> of the named property, or null.</summary>
  public RppValue? GetPropertyValue(string name, int index)
  {
    return GetProperty(name)?[index];
  }

  /// <summary>
  /// Sets the value at <paramref name="index"/> of the named property.
  /// Creates the property when absent; pads with <c>0</c> when index exceeds current length.
  /// </summary>
  public void SetPropertyValue(string name, int index, RppValue value)
  {
    var prop = GetProperty(name);
    if (prop is null)
    {
      prop = new RppProperty(name);
      Entries.Add(prop);
    }

    while (prop.Values.Count <= index)
    {
      prop.Values.Add(RppValue.From(0));
    }

    prop.Values[index] = value;
  }

  /// <summary>First multi-line text block in this node, or null.</summary>
  public RppMultilineText? GetMultilineText()
  {
    return Entries.OfType<RppMultilineText>().FirstOrDefault();
  }

  public override string ToString()
  {
    if (DefaultValues.Count > 0)
    {
      return $"{Name} {string.Join(' ', DefaultValues)}";
    }

    return Name;
  }
}
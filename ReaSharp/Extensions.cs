namespace ReaSharp;

public static class Extensions
{
  public static bool IsWithin(this TimeSpan sp, IArrangeItem item)
  {
    return
      sp.TotalSeconds >= item.Start.TotalSeconds &&
      sp.TotalSeconds <= item.End.TotalSeconds;
  }

  public static string TryGet(this IList<string> tokens, int index)
  {
    if (index < 0 || index >= tokens.Count) return string.Empty;
    return tokens[index];
  }
}
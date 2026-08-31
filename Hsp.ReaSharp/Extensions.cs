namespace Hsp.ReaSharp;

public static class Extensions
{
  public static bool IsWithin(this TimeSpan sp, IArrangeItem item)
  {
    return
      sp.TotalSeconds >= item.Start.TotalSeconds &&
      sp.TotalSeconds <= item.End.TotalSeconds;
  }

  public static T? TryGet<T>(this IList<T> tokens, int index)
  {
    if (index < 0 || index >= tokens.Count) return default;
    return tokens[index];
  }
}
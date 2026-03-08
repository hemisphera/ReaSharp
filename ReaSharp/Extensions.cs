namespace ReaSharp;

public static class Extensions
{
  public static bool IsWithin(this TimeSpan sp, IArrangeItem item)
  {
    return
      sp.TotalSeconds >= item.Start.TotalSeconds &&
      sp.TotalSeconds <= item.End.TotalSeconds;
  }
}
using System.Reflection;
using System.Text;

namespace ReaSharp.Test;

internal static class Helper
{
  public static Stream GetResource(params string[] parts)
  {
    var sourcePath = new FileInfo(Assembly.GetExecutingAssembly().Location);
    var folder = Path.Combine(sourcePath.Directory?.FullName ?? string.Empty, "Resources");
    var partsList = parts.ToList();
    partsList.Insert(0, folder);
    return File.Open(Path.Combine(partsList.ToArray()), FileMode.Open);
  }

  public static string GetResourceAsText(params string[] parts)
  {
    using var s = GetResource(parts);
    using var sr = new StreamReader(s, Encoding.UTF8);
    return sr.ReadToEnd();
  }
}
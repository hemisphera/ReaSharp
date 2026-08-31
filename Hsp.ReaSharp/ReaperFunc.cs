using System.Runtime.InteropServices;

namespace Hsp.ReaSharp;

/// <summary>
/// A lazily-loaded REAPER function pointer. Resolves and caches the native delegate on first access.
/// Throws <see cref="NotSupportedException"/> if the function is unavailable in the running REAPER version.
/// </summary>
/// <remarks>
/// Must be stored as a static <b>field</b> (not a property) in <see cref="Reaper"/> so that the cached
/// delegate is written back to the original storage location rather than a copy.
/// </remarks>
public struct ReaperFunc<T> where T : Delegate
{
  private readonly string[] _names;

  public ReaperFunc(params string[] names) => _names = names;

  /// <summary>Returns the resolved delegate, loading and caching it on first call.</summary>
  public T Invoke
  {
    get
    {
      if (field is not null) return field;
      foreach (var name in _names)
      {
        var ptr = Marshal.StringToHGlobalAnsi(name);
        var funcPtr = Reaper.GetFunc(ptr);
        Marshal.FreeHGlobal(ptr);
        if (funcPtr != nint.Zero)
          return field = Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
      }

      throw new NotSupportedException($"REAPER function '{string.Join(" / ", _names)}' is not available in this version.");
    }
  }

  /// <summary>
  /// Eagerly resolves and caches the function pointer.
  /// Call this during plugin initialization to fail fast if the function is unavailable.
  /// </summary>
  /// <exception cref="NotSupportedException">Thrown if the function is not available in this REAPER version.</exception>
  public void Load() => _ = Invoke;
}
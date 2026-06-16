namespace ReaSharp;

public abstract class ReaperObject : IEquatable<ReaperObject>
{
  public abstract nint ReaperHandle { get; }

  public bool Equals(ReaperObject? other) => other is not null && ReaperHandle == other.ReaperHandle;
  public override bool Equals(object? obj) => obj is ReaperObject other && Equals(other);
  public override int GetHashCode() => ReaperHandle.GetHashCode();
  public static bool operator ==(ReaperObject? left, ReaperObject? right) => left?.ReaperHandle == right?.ReaperHandle;
  public static bool operator !=(ReaperObject? left, ReaperObject? right) => !(left == right);
}
using System.Runtime.InteropServices;

namespace ReaSharp;

public sealed class GmemService : IGmemService
{
  private const int NamedGmemBlocks = 512;
  private const int ItemsPerBlockLog2 = 16;
  private const int ItemsPerBlock = 1 << ItemsPerBlockLog2;
  private const int ItemMask = ItemsPerBlock - 1;
  private const int MaxNamedSlots = NamedGmemBlocks * ItemsPerBlock;

  private IntPtr _gmem;

  public string? ConnectedName { get; private set; }
  public bool IsConnected => _gmem != IntPtr.Zero;

  public void Connect(string name, bool isAlloc = true)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("gmem name must not be empty.", nameof(name));

    var namePtr = Marshal.StringToHGlobalAnsi(name);
    try
    {
      _gmem = Reaper.eel_gmem_attach(namePtr, isAlloc);
    }
    finally
    {
      Marshal.FreeHGlobal(namePtr);
    }

    if (_gmem == IntPtr.Zero)
      throw new Exception($"Unable to attach to gmem '{name}'.");

    ConnectedName = name;
  }

  public double Read(int index)
  {
    EnsureConnected();
    EnsureIndex(index);

    Reaper.NSEEL_HOSTSTUB_EnterMutex();
    try
    {
      unsafe
      {
        var blocks = *(double***)_gmem;
        if (blocks == null) return 0.0;
        var block = blocks[index >> ItemsPerBlockLog2];
        return block == null ? 0.0 : block[index & ItemMask];
      }
    }
    finally
    {
      Reaper.NSEEL_HOSTSTUB_LEAVEMutex();
    }
  }

  public void Write(int index, double value)
  {
    EnsureConnected();
    EnsureIndex(index);

    Reaper.NSEEL_HOSTSTUB_EnterMutex();
    try
    {
      unsafe
      {
        var blocks = *(double***)_gmem;
        if (blocks == null) throw new Exception("gmem block table is not available.");
        var block = blocks[index >> ItemsPerBlockLog2];
        if (block == null) throw new Exception($"gmem block for index {index} is not allocated.");
        block[index & ItemMask] = value;
      }
    }
    finally
    {
      Reaper.NSEEL_HOSTSTUB_LEAVEMutex();
    }
  }

  private void EnsureConnected()
  {
    if (!IsConnected)
      throw new Exception("No gmem instance is connected. Call Connect(name) first.");
  }

  private static void EnsureIndex(int index)
  {
    if (index < 0 || index >= MaxNamedSlots)
      throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be in range 0..{MaxNamedSlots - 1}.");
  }
}

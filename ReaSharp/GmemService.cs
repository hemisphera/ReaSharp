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
  public bool IsConnected => _gmem != nint.Zero;


  public void Connect(string name, bool isAlloc = true)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("gmem name must not be empty.", nameof(name));

    var namePtr = Marshal.StringToHGlobalAnsi(name);
    try
    {
      _gmem = Reaper.eel_gmem_attach.Invoke(namePtr, isAlloc);
    }
    finally
    {
      Marshal.FreeHGlobal(namePtr);
    }

    if (_gmem == nint.Zero)
      throw new Exception($"Unable to attach to gmem '{name}'.");

    ConnectedName = name;
  }

  public void Disconnect()
  {
    EnsureConnected();
    ConnectedName = null;
    _gmem = nint.Zero;
  }

  public double Read(int index)
  {
    var buf = new double[1];
    return Read(index, buf) ? buf[0] : 0.0;
  }

  public bool Read(int index, double[] buffer)
  {
    EnsureConnected();

    Reaper.NSEEL_HOSTSTUB_EnterMutex.Invoke();
    try
    {
      unsafe
      {
        var blocks = *(double***)_gmem;
        if (blocks == null) return false;

        for (var i = 0; i < buffer.Length; i++)
        {
          var actualIndex = index + i;
          EnsureIndex(actualIndex);
          var block = blocks[actualIndex >> ItemsPerBlockLog2];
          buffer[i] = block == null ? 0.0 : block[actualIndex & ItemMask];
        }
      }

      return true;
    }
    finally
    {
      Reaper.NSEEL_HOSTSTUB_LEAVEMutex.Invoke();
    }
  }

  public void Write(int index, double value)
  {
    Write(index, [value]);
  }

  public void Write(int index, IEnumerable<double> buffer)
  {
    EnsureConnected();

    Reaper.NSEEL_HOSTSTUB_EnterMutex.Invoke();
    try
    {
      var bufferArray = buffer.ToArray();
      unsafe
      {
        var blocks = *(double***)_gmem;
        if (blocks == null) throw new Exception("gmem block table is not available.");
        for (var i = 0; i < bufferArray.Length; i++)
        {
          var actualIndex = index + i;
          EnsureIndex(actualIndex);
          var block = blocks[actualIndex >> ItemsPerBlockLog2];
          if (block == null) throw new Exception($"gmem block for index {actualIndex} is not allocated.");
          block[actualIndex & ItemMask] = bufferArray[i];
        }
      }
    }
    finally
    {
      Reaper.NSEEL_HOSTSTUB_LEAVEMutex.Invoke();
    }
  }

  private void EnsureConnected()
  {
    if (!IsConnected)
      throw new Exception("No gmem instance is connected. Call Connect(name) first.");
  }

  private static void EnsureIndex(int index)
  {
    if (index is < 0 or >= MaxNamedSlots)
      throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be in range 0..{MaxNamedSlots - 1}.");
  }
}
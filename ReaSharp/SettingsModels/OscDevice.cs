using ReaSharp.RppXml;

namespace ReaSharp.SettingsModels;

public class OscDevice
{
  public static OscDevice? Parse(string? line)
  {
    var tokens = RppReader.Tokenize(line ?? string.Empty, true);
    if (tokens.TryGet(0) != "OSC") return null;
    return new OscDevice
    {
      Name = tokens.TryGet(1),
      Mode = int.TryParse(tokens.TryGet(2), out var mode) ? mode : 0,
      ReaperPort = int.TryParse(tokens.TryGet(3), out var reaperPort) ? reaperPort : 0,
      DeviceIp = tokens.TryGet(4),
      DevicePort = int.TryParse(tokens.TryGet(5), out var devicePort) ? devicePort : 0,
      MaxPacketSize = int.TryParse(tokens.TryGet(6), out var maxPacketSize) ? maxPacketSize : 0,
      WaitTime = int.TryParse(tokens.TryGet(7), out var waitTime) ? waitTime : 0,
      Definition = tokens.TryGet(8)
    };
  }
  
  public string Name { get; set; } = string.Empty;
  public int Mode { get; set; }
  public int ReaperPort { get; set; }
  public string DeviceIp { get; set; } = string.Empty;
  public int DevicePort { get; set; }
  public int MaxPacketSize { get; set; }
  public int WaitTime { get; set; }
  public string Definition { get; set; } = string.Empty;
}
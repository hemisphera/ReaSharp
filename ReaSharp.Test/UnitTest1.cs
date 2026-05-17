using ReaSharp.RppXml;
using ReaSharp.RppXml.Nodes;

namespace ReaSharp.Test;

public class UnitTest1
{
  [Fact]
  public void ReadRppFile()
  {
    var cont = Helper.GetResourceAsText("TestFile.rpp");
    var nodes = RppReader.ReadFromString(cont);
    var text = nodes.FindChild<RppNotesNode>()?.Text;
  }
}
namespace ReaSharp.RppXml.Nodes;

public class RppNotesNode : RppNode
{
  public string[] Lines
  {
    get => Entries.OfType<RppMultilineText>().FirstOrDefault()?.Lines.ToArray() ?? [];
    set
    {
      var mlText = Entries.OfType<RppMultilineText>().FirstOrDefault();
      if (mlText == null)
      {
        mlText = new RppMultilineText();
        Entries.Add(mlText);
      }

      mlText.Lines.Clear();
      mlText.Lines.AddRange(value);
    }
  }

  public string Text
  {
    get => string.Join(Environment.NewLine, Lines);
    set => Lines = value.Split(Environment.NewLine);
  }

  public RppNotesNode() : base("NOTES")
  {
  }
}
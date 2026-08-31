namespace Hsp.ReaSharp.Models;

public class TrackTreeItem
{
  public static TrackTreeItem[] Build(Project project)
  {
    return Build(project.GetTracks());
  }

  public static TrackTreeItem[] Build(IEnumerable<Track> tracks)
  {
    var roots = new List<TrackTreeItem>();
    // Stack tracks the current parent chain; each entry is the TrackTreeItem
    // whose children we are currently appending to.
    var parentStack = new Stack<TrackTreeItem>();

    foreach (var track in tracks)
    {
      var level = parentStack.Count;
      var parent = parentStack.Count > 0 ? parentStack.Peek() : null;
      var item = new TrackTreeItem(parent, track, level);

      if (parent == null)
        roots.Add(item);
      else
        parent.Children.Add(item);

      var depth = track.FolderLevel;
      if (depth > 0)
      {
        // This track opens folder level(s); push it as the new parent.
        // REAPER only ever uses +1, but handle >1 just in case.
        for (var i = 0; i < depth; i++)
          parentStack.Push(item);
      }
      else if (depth < 0)
      {
        // Close |depth| levels: pop that many parents.
        for (var i = 0; i < -depth; i++)
        {
          if (parentStack.Count == 0)
            return [.. roots];
          parentStack.Pop();
        }
      }
    }

    return [.. roots];
  }

  public TrackTreeItem? Parent { get; }
  public Track Track { get; }
  public List<TrackTreeItem> Children { get; } = [];
  public int Level { get; }

  public IEnumerable<TrackTreeItem> FlattenChildren(Predicate<TrackTreeItem>? predicate = null)
  {
    var result = new List<TrackTreeItem>();
    Recurse(predicate, result);
    return result;
  }

  private void Recurse(Predicate<TrackTreeItem>? predicate, IList<TrackTreeItem> found)
  {
    foreach (var item in Children)
    {
      if (predicate == null || predicate(item))
        found.Add(item);
      item.Recurse(predicate, found);
    }
  }


  public TrackTreeItem(TrackTreeItem? parent, Track track, int level)
  {
    Parent = parent;
    Track = track;
    Level = level;
  }
}
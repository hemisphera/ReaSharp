using ReaSharp.Models;

namespace ReaSharp;

public interface ICommandRegistry
{
  ReaperCommand Register(string uniqueName, string description, Func<IServiceProvider, Task> handler);
  ReaperCommand? GetById(int command);
}
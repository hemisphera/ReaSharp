using Hsp.ReaSharp.Models;

namespace Hsp.ReaSharp;

public interface ICommandRegistry
{
  ReaperCommand Register(string uniqueName, string description, Func<IServiceProvider, ActionContext, Task> handler);
  ReaperCommand? GetById(int command);
}
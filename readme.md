# ReaSharp - Write REAPER plugins in C#/.NET

## Quick start

- Create a new .NET10 class library project like this:

    ````
    ...
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <!-- Remember: REAPER expects plugins as "reaper_xxx.dll" -->
        <AssemblyName>reaper_zulbert</AssemblyName>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <!-- Because we need to do nasty low-level stuff -->
        <PublishAot>true</PublishAot>
        <!-- Because we need to do nasty low-level stuff -->
        <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
        <!-- Because we need to correctly expose [UnmanagedCallersOnly] -->
        <NativeLib>Shared</NativeLib>
    </PropertyGroup>
    ...
    ````
- Reference `ReaSharp` as project or NuGet package
- Create a static class `Plugin` (or whatever) as the entry point:
  ````
  [UnmanagedCallersOnly(EntryPoint = "ReaperPluginEntry")]
  public static int ReaperPluginEntry(IntPtr hInstance, IntPtr rec)
  {
    try
    {
      // Initialize the PluginState. Must do this first thing. This will load
      // REAPER functions and initialize things.
      PluginState.Initialize(ReaperPluginInfo.FromPointer(rec));
      // Add a command regitry, if you plan to add commands. Bring your own or use
      // the default one.
      var cr = PluginState.Instance.AddCommandRegistry(new DefaultCommandRegistry());

      // yay!
      return 1;
    }
    catch
    {
      // nay!
      return 0;
    }
  }
  ````
- Publish your project using something like `dotnet publish -r win-x64 -c Release`
- Copy the file to `%APPDATA%\REAPER\UserPlugins`
- Enjoy

## Architecture

This here plugin is structured somewhat similar to what Helgoboss has done with https://github.com/helgoboss/reaper-rs. A low-level and a high-level API:

### Low-Level API

The static class `Reaper.cs` is mostly auto-generated from REAPERs API documentation that lives at https://www.reaper.fm/sdk/reascript/reascripthelp.html. Most (if not all) of the API that REAPER provides is mapped as an unsafe low-level import in that static class. There is a script in `Scripts\Generate-ReaperImports.ps1` that regenerates/updates the definitions. Feel free to use this as you want. It's a bit cumbersome though.

### High-Level API

This is an object model (found in `Models` namespace mostly) that provides a more .NET-y way to access REAPER and hides most of the ugly stuff that needs to happen in the low-level API from you. Of course implementing all of the various parts is quite a task so this will proceed as parts are needed.

### Utilities

There are also some utilities that don't really map to REAPERs object model but they come in handy for various scenarios:

- `TrackTreeItem` provides a tree-like model for tracks to provide hierarchical access to parent/child tracks.
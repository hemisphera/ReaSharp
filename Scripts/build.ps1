dotnet publish -r win-x64 -c Release
Copy-Item -Path "$PSScriptRoot\..\ReaSharp\bin\Release\net10.0\win-x64\publish\reaper_reasharp.dll" -Destination "$env:APPDATA\REAPER\UserPlugins" -Force
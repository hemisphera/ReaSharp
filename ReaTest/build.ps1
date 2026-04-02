$ErrorActionPreference = "Stop"
dotnet publish .\ReaTest.csproj -r win-x64 -c Release
$file = Get-ChildItem "$PSScriptRoot\bin\Release\net10.0\win-x64\publish" -Filter "reaper_*.dll"
Copy-Item -Path $file -Destination "$env:APPDATA\REAPER\UserPlugins" -Force
& "C:\Program Files\REAPER (x64)\reaper.exe"
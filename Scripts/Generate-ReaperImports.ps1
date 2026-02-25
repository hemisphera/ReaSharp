[CmdletBinding()]
param(
  [string]$ApiUrl = 'https://www.reaper.fm/sdk/reascript/reascripthelp.html',
  [string]$ReaperFile = (Join-Path $PSScriptRoot '..\ReaSharp\Reaper.cs')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Convert-CTypeToCSharp {
  param(
    [Parameter(Mandatory = $true)]
    [string]$CType
  )

  $typeText = $CType.Trim()
  $pointerCount = ([regex]::Matches($typeText, '\*')).Count

  $normalized = $typeText -replace '\bconst\b', ''
  $normalized = $normalized -replace '\bvolatile\b', ''
  $normalized = $normalized -replace '\bstruct\b', ''
  $normalized = $normalized -replace '\benum\b', ''
  $normalized = $normalized -replace '\*', ''
  $normalized = ($normalized -replace '\s+', ' ').Trim().ToLowerInvariant()

  if ($pointerCount -gt 0) {
    return 'IntPtr'
  }

  switch ($normalized) {
    'void' { return 'void' }
    'bool' { return 'bool' }
    'char' { return 'byte' }
    'short' { return 'short' }
    'unsigned short' { return 'ushort' }
    'int' { return 'int' }
    'unsigned int' { return 'uint' }
    'long' { return 'long' }
    'unsigned long' { return 'ulong' }
    'long long' { return 'long' }
    'unsigned long long' { return 'ulong' }
    'float' { return 'float' }
    'double' { return 'double' }
    'size_t' { return 'nuint' }
    'intptr_t' { return 'nint' }
    'uintptr_t' { return 'nuint' }
    default { return 'IntPtr' }
  }
}

function Escape-CSharpIdentifier {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Name
  )

  $keywords = @{
    'abstract' = $true; 'as' = $true; 'base' = $true; 'bool' = $true; 'break' = $true; 'byte' = $true;
    'case' = $true; 'catch' = $true; 'char' = $true; 'checked' = $true; 'class' = $true; 'const' = $true;
    'continue' = $true; 'decimal' = $true; 'default' = $true; 'delegate' = $true; 'do' = $true; 'double' = $true;
    'else' = $true; 'enum' = $true; 'event' = $true; 'explicit' = $true; 'extern' = $true; 'false' = $true;
    'finally' = $true; 'fixed' = $true; 'float' = $true; 'for' = $true; 'foreach' = $true; 'goto' = $true;
    'if' = $true; 'implicit' = $true; 'in' = $true; 'int' = $true; 'interface' = $true; 'internal' = $true;
    'is' = $true; 'lock' = $true; 'long' = $true; 'namespace' = $true; 'new' = $true; 'null' = $true;
    'object' = $true; 'operator' = $true; 'out' = $true; 'override' = $true; 'params' = $true; 'private' = $true;
    'protected' = $true; 'public' = $true; 'readonly' = $true; 'ref' = $true; 'return' = $true; 'sbyte' = $true;
    'sealed' = $true; 'short' = $true; 'sizeof' = $true; 'stackalloc' = $true; 'static' = $true; 'string' = $true;
    'struct' = $true; 'switch' = $true; 'this' = $true; 'throw' = $true; 'true' = $true; 'try' = $true;
    'typeof' = $true; 'uint' = $true; 'ulong' = $true; 'unchecked' = $true; 'unsafe' = $true; 'ushort' = $true;
    'using' = $true; 'virtual' = $true; 'void' = $true; 'volatile' = $true; 'while' = $true
  }

  if ($keywords.ContainsKey($Name)) {
    return "@$Name"
  }

  return $Name
}

function Parse-Parameters {
  param(
    [Parameter(Mandatory = $true)]
    [AllowEmptyString()]
    [string]$ParameterText
  )

  $trimmed = $ParameterText.Trim()
  if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed -eq 'void') {
    return @()
  }

  $parts = $trimmed -split ','
  $parameters = [System.Collections.Generic.List[string]]::new()
  $index = 1

  foreach ($rawPart in $parts) {
    $part = ($rawPart -replace '\s+', ' ').Trim()
    if ([string]::IsNullOrWhiteSpace($part) -or $part -eq '...') {
      continue
    }

    $paramType = $part
    $paramName = "arg$index"

    if ($part -match '^(?<type>.+?)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)$') {
      $paramType = $Matches.type.Trim()
      $paramName = $Matches.name.Trim()
    }

    $paramName = Escape-CSharpIdentifier -Name $paramName
    $csType = Convert-CTypeToCSharp -CType $paramType
    $parameters.Add("$csType $paramName")

    $index++
  }

  return $parameters
}

function Build-GeneratedContent {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Signatures
  )

  $delegates = [System.Collections.Generic.List[string]]::new()
  $properties = [System.Collections.Generic.List[string]]::new()
  $loads = [System.Collections.Generic.List[string]]::new()

  foreach ($signatureRaw in $Signatures) {
    $signature = ($signatureRaw -replace '\s+', ' ').Trim()
    if ($signature -notmatch '^(?<ret>.+?)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\((?<params>.*)\)$') {
      continue
    }

    $returnType = Convert-CTypeToCSharp -CType $Matches.ret
    $functionName = $Matches.name
    $parameterList = Parse-Parameters -ParameterText $Matches.params
    if ($null -eq $parameterList) {
      $parameterList = @()
    }
    $parametersJoined = [string]::Join(', ', @($parameterList))

    $isUnsafe = $returnType.Contains('*') -or ($parameterList | Where-Object { $_ -match '\*\s+[A-Za-z_@]' } | Measure-Object).Count -gt 0
    $unsafeToken = if ($isUnsafe) { ' unsafe' } else { '' }

    $delegates.Add('[UnmanagedFunctionPointer(CallingConvention.Cdecl)]')
    $delegates.Add("public${unsafeToken} delegate $returnType ${functionName}Delegate($parametersJoined);")
    $delegates.Add('')

    $properties.Add("  public static ${functionName}Delegate $functionName { get; private set; } = null!;")
    $loads.Add("    $functionName = LoadFunction<${functionName}Delegate>(nameof($functionName));")
  }

  if ($delegates.Count -gt 0 -and [string]::IsNullOrWhiteSpace($delegates[$delegates.Count - 1])) {
    $delegates.RemoveAt($delegates.Count - 1)
  }

  return [pscustomobject]@{
    Delegates = $delegates
    Properties = $properties
    Loads = $loads
  }
}

function Replace-Region {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Text,
    [Parameter(Mandatory = $true)]
    [string]$StartMarker,
    [Parameter(Mandatory = $true)]
    [string]$EndMarker,
    [Parameter()]
    [string[]]$Content
  )

  if ($null -eq $Content) {
    $Content = @()
  }

  $pattern = [regex]::Escape($StartMarker) + '.*?' + [regex]::Escape($EndMarker)
  $replacementLines = @($StartMarker)
  if ($Content.Count -gt 0) {
    $replacementLines += $Content
  }
  $replacementLines += $EndMarker

  $replacement = [string]::Join("`r`n", $replacementLines)
  $regex = [regex]::new($pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

  if (-not $regex.IsMatch($Text)) {
    throw "Marker region not found: $StartMarker"
  }

  return $regex.Replace($Text, $replacement, 1)
}

Write-Host "Downloading REAPER API docs from $ApiUrl"
$response = Invoke-WebRequest -Uri $ApiUrl
$html = $response.Content

$signatureMatches = [regex]::Matches(
  $html,
  '<div class="c_func">.*?<code>(?<sig>.*?)</code><br><br></div>',
  [System.Text.RegularExpressions.RegexOptions]::Singleline
)

if ($signatureMatches.Count -eq 0) {
  throw 'No C API signatures were found in the documentation.'
}

$signatures = [System.Collections.Generic.List[string]]::new()
foreach ($match in $signatureMatches) {
  $decoded = [System.Net.WebUtility]::HtmlDecode($match.Groups['sig'].Value)
  $decoded = [regex]::Replace($decoded, '<.*?>', '')
  $decoded = ($decoded -replace '\s+', ' ').Trim()
  if (-not [string]::IsNullOrWhiteSpace($decoded)) {
    $signatures.Add($decoded)
  }
}

$generated = Build-GeneratedContent -Signatures $signatures

if (-not (Test-Path -Path $ReaperFile)) {
  throw "Target file not found: $ReaperFile"
}

$reaperText = Get-Content -Path $ReaperFile -Raw

$reaperText = Replace-Region -Text $reaperText -StartMarker '// <auto-generated-reaper-delegates>' -EndMarker '// </auto-generated-reaper-delegates>' -Content $generated.Delegates
$reaperText = Replace-Region -Text $reaperText -StartMarker '  // <auto-generated-reaper-properties>' -EndMarker '  // </auto-generated-reaper-properties>' -Content $generated.Properties
$reaperText = Replace-Region -Text $reaperText -StartMarker '    // <auto-generated-reaper-loads>' -EndMarker '    // </auto-generated-reaper-loads>' -Content $generated.Loads

Set-Content -Path $ReaperFile -Value $reaperText -Encoding UTF8

Write-Host "Generated imports for $($generated.Properties.Count) REAPER API functions in $ReaperFile"
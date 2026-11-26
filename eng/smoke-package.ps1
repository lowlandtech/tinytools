<#
.SYNOPSIS
    Verifies the behaviour of a published package, not of the source tree.

.DESCRIPTION
    Source tests and a green acceptance cannot prove what a published artifact does: every test in this
    repository writes its input the way the test author wrote it, and a consumer does not. That gap is not
    theoretical - it hid a watcher that rendered an edit twice until a throwaway consumer project found it
    after 953 tests had passed.

    This script builds a consumer in a temporary directory, references the package from the feed exactly as
    a consumer would, and asserts the watcher contract:

      * a single write renders once;
      * a write that pauses inside the settle window renders once;
      * a write that pauses past the settle window renders twice, which is the documented boundary;
      * an unrelated file change renders zero times.

    It writes nothing to the repository and removes its consumer when it finishes.

.PARAMETER Feed
    The feed the consumer restores from. Defaults to the workspace feed, D:/lowlandtech/packages.

.PARAMETER Version
    The package version to verify. Defaults to the version Directory.Build.props declares.

.PARAMETER KeepConsumer
    Leaves the generated consumer directory in place, for inspecting a failure.

.EXAMPLE
    pwsh eng/smoke-package.ps1
#>
[CmdletBinding()]
param(
    [string] $Feed = 'D:/lowlandtech/packages',
    [string] $Version,
    [switch] $KeepConsumer
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot

function Get-DeclaredVersion {
    [xml] $document = Get-Content (Join-Path $repositoryRoot 'Directory.Build.props') -Raw
    $declared = $document.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    if (-not $declared) { throw "Directory.Build.props declares no Version property." }
    return [string] $declared
}

if (-not $Version) { $Version = Get-DeclaredVersion }
if (-not (Test-Path $Feed)) { throw "The feed $Feed does not exist. Pack first with eng/pack-feed.ps1." }

$package = Join-Path $Feed "LowlandTech.TinyTools.$Version.nupkg"
if (-not (Test-Path $package)) {
    throw "The feed $Feed carries no LowlandTech.TinyTools $Version. Pack it first with eng/pack-feed.ps1."
}

$consumer = Join-Path ([System.IO.Path]::GetTempPath()) ("tinytools-smoke-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $consumer | Out-Null

try {
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="LowlandTech.TinyTools" Version="$Version" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path (Join-Path $consumer 'Smoke.csproj') -Encoding utf8

    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="workspace" value="$($Feed -replace '\\', '\\')" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -Path (Join-Path $consumer 'NuGet.config') -Encoding utf8

    $program = Join-Path $PSScriptRoot 'smoke-package.consumer.cs.txt'
    if (-not (Test-Path $program)) { throw "The consumer program is missing: $program" }
    Copy-Item -Path $program -Destination (Join-Path $consumer 'Program.cs')

    # The consumer is generated outside the repository, so it inherits no SDK pin of its own and the
    # machine default would be selected - a newer SDK than this repository requires. Carrying the
    # repository's global.json across makes the gate verify the package on the SDK it will build with.
    $globalJson = Join-Path $repositoryRoot 'global.json'
    if (Test-Path $globalJson) { Copy-Item -Path $globalJson -Destination (Join-Path $consumer 'global.json') }

    Push-Location $consumer
    try { $selectedSdk = (& dotnet --version) } finally { Pop-Location }
    Write-Host "Selected SDK in the consumer: $selectedSdk"
    Write-Host "Verifying LowlandTech.TinyTools $Version from $Feed"
    Push-Location $consumer
    try {
        & dotnet run --configuration Release
        $exit = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    if ($exit -ne 0) { throw "The published package $Version failed its consumer smoke with exit code $exit." }

    Write-Host "The published package $Version behaves as documented."
}
finally {
    if ($KeepConsumer) { Write-Host "Consumer kept at $consumer" }
    else { Remove-Item -Recurse -Force $consumer -ErrorAction SilentlyContinue }
}

<#
.SYNOPSIS
    Packs the shipped packages into the workspace feed without overwriting a published version.

.DESCRIPTION
    A package version identifies content. Repacking the same version with different content
    destroys that identity: a consumer that pinned the version plus a digest then fails with an
    InvalidDataException before any product code runs, and the original artifact is gone.

    So this script never replaces an archive that already exists. It packs the four shipped
    projects at the version declared by `Directory.Build.props`, and if any archive for that
    version is already in the feed it stops before writing anything and names the archive, its
    published digest and the digest it just built. The recovery is always the same: bump the
    version with `eng/set-version.ps1`, then pack.

    The comparison is deliberately not "same bytes means safe": `dotnet pack` is not
    byte-deterministic on this toolchain (two packs of one tree produce different archives), so a
    byte-equality test would both miss real content changes and refuse harmless ones. A published
    version is immutable; that is the rule this script enforces.

.PARAMETER Feed
    The feed directory. Defaults to the workspace feed, D:/lowlandtech/packages.

.EXAMPLE
    pwsh eng/pack-feed.ps1
#>
[CmdletBinding()]
param(
    [string] $Feed = 'D:/lowlandtech/packages'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot

function Get-DeclaredVersion {
    $props = Join-Path $repositoryRoot 'Directory.Build.props'
    [xml] $document = Get-Content $props -Raw
    $version = $document.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    if (-not $version) { throw "Directory.Build.props declares no Version property." }
    return [string] $version
}

function Get-Sha256 {
    param([Parameter(Mandatory = $true)] [string] $Path)
    return (Get-FileHash -Path $Path -Algorithm SHA256).Hash
}

$version = Get-DeclaredVersion
$projects = @(
    'src/lib/LowlandTech.TinyTools.csproj',
    'src/core/LowlandTech.TinyTools.Core.csproj',
    'src/frontend/LowlandTech.TinyTools.Frontend.csproj',
    'src/api/LowlandTech.TinyTools.Api.csproj'
)

New-Item -ItemType Directory -Force -Path $Feed | Out-Null

# Pack into a staging directory first, so the feed is untouched until every archive exists.
$staging = Join-Path ([System.IO.Path]::GetTempPath()) ("tinytools-pack-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $staging | Out-Null

try {
    foreach ($project in $projects) {
        $path = Join-Path $repositoryRoot $project
        if (-not (Test-Path $path)) { throw "The project $project does not exist." }

        Write-Host "Packing $project at $version"
        & dotnet pack $path --configuration Release --output $staging --nologo
        if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed for $project with exit code $LASTEXITCODE." }
    }

    $refused = @()
    $written = @()

    foreach ($archive in Get-ChildItem -Path $staging -Filter '*.nupkg' | Sort-Object Name) {
        $destination = Join-Path $Feed $archive.Name
        if (Test-Path $destination) {
            $refused += ("{0}: published {1}, this build produced {2}" -f
                $archive.Name,
                (Get-Sha256 -Path $destination).Substring(0, 16),
                (Get-Sha256 -Path $archive.FullName).Substring(0, 16))
            continue
        }

        Move-Item -Path $archive.FullName -Destination $destination
        $written += "$($archive.Name) $((Get-Sha256 -Path $destination).Substring(0, 16))"
    }

    foreach ($entry in $written) { Write-Host "Wrote   $entry" }

    if ($refused.Count -gt 0) {
        Write-Host ''
        foreach ($entry in $refused) { Write-Host "REFUSED $entry" }
        throw ("Version $version is already published in $Feed and a version is immutable. " +
               "Bump it with eng/set-version.ps1 and pack again. Nothing was overwritten.")
    }

    Write-Host "Feed $Feed now carries version $version."
}
finally {
    Remove-Item -Recurse -Force $staging -ErrorAction SilentlyContinue
}

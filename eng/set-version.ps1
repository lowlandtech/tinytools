<#
.SYNOPSIS
    Sets the repository's single version and updates the surfaces derived from it.

.DESCRIPTION
    `Directory.Build.props` owns the version. This script changes it there and updates the
    readme badge, which is the only other hand-maintained surface. Everything else follows
    the same value:

      * package version  - derived automatically by MSBuild from Directory.Build.props
      * release tag      - `v<version>`, checked by the tags workflow before it packs
      * readme badge     - rewritten here
      * changelog        - the release heading must be `# <version>`

.PARAMETER Version
    The version to set, for example 2026.10.3.

.EXAMPLE
    pwsh eng/set-version.ps1 -Version 2026.10.3
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d{4}\.\d+\.\d+([-.+][0-9A-Za-z.-]+)?$')]
    [string] $Version
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Write-Verbose "Repository root: $root"

$propsPath = Join-Path $root 'Directory.Build.props'
$props = Get-Content -Raw $propsPath
$current = [regex]::Match($props, '<Version>[^<]+</Version>')
if (-not $current.Success) { throw "Directory.Build.props has no <Version> element to update." }

$props.Replace($current.Value, "<Version>$Version</Version>") |
    Set-Content -Path $propsPath -NoNewline -Encoding utf8

$readmePath = Join-Path $root 'readme.md'
$readme = Get-Content -Raw $readmePath
$badge = [regex]::Match($readme, 'badge/version-[0-9A-Za-z.-]+-blue')
if (-not $badge.Success) { throw "readme.md has no version badge in the expected shape." }

$readme.Replace($badge.Value, "badge/version-$Version-blue") |
    Set-Content -Path $readmePath -NoNewline -Encoding utf8

$changelogPath = Join-Path $root 'changelog.md'
$changelog = Get-Content -Raw $changelogPath
if ($changelog -notmatch "(?m)^# $([regex]::Escape($Version))\b") {
    Write-Warning "changelog.md has no '# $Version' heading yet; add the release note before tagging."
}

Write-Host "Version set to $Version in Directory.Build.props and readme.md."
Write-Host "Next: add the changelog heading, commit, then tag v$Version."

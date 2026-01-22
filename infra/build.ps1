##########################################################################
# TinyTools Build Script
# Automatic versioning with BUILDR file
##########################################################################

[CmdletBinding()]
Param(
    [ValidateSet("Build", "Test", "Pack", "Push", "CI", "Clean")]
    [string]$Target = "CI",
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [switch]$NoBump
)

$ErrorActionPreference = "Stop"

# Paths - using script scope for functions to access
$script:rootDir = Split-Path $PSScriptRoot -Parent
$script:solutionPath = Join-Path $script:rootDir "tinytools.sln"
$script:projectPath = Join-Path $script:rootDir "src\lowlandtech.tinytools\LowlandTech.TinyTools.csproj"
$script:buildRFile = Join-Path $PSScriptRoot "BUILDR"
$script:artifactsDir = Join-Path $script:rootDir "artifacts"
$script:bagetUrl = "http://localhost:5000/v3/index.json"
$script:bagetApiKey = "BAGET-SERVER-API-KEY"
$buildRFile = Join-Path $PSScriptRoot "BUILDR"
$artifactsDir = Join-Path $rootDir "artifacts"
$bagetUrl = "http://localhost:5000/v3/index.json"

function Get-BaseVersion {
    [xml]$csproj = Get-Content $projectPath
    $version = $csproj.Project.PropertyGroup.Version
    if (-not $version) {
        throw "No Version found in $projectPath"
    }
    return $version
}

function Get-BuildNumber {
    if (Test-Path $buildRFile) {
        return [int](Get-Content $buildRFile -Raw).Trim()
    }
    return 0
}

function Increment-BuildNumber {
    $buildNum = Get-BuildNumber
    $buildNum++
    $buildNum | Set-Content $buildRFile -NoNewline
    return $buildNum
}

function Get-DerivedVersion {
    $baseVersion = Get-BaseVersion
    $buildNum = Get-BuildNumber
    
    # Replace last segment with build number: 2026.1.0 -> 2026.1.5
    $parts = $baseVersion -split '\.'
    if ($parts.Count -ge 3) {
        $parts[-1] = $buildNum
    }
    
    return $parts -join '.'
}

function Clean {
    Write-Host "🧹 Cleaning..." -ForegroundColor Cyan
    
    Remove-Item "$rootDir\src\**\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$rootDir\src\**\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$rootDir\test\**\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$rootDir\test\**\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $artifactsDir -Recurse -Force -ErrorAction SilentlyContinue
    
    Write-Host "✅ Clean complete" -ForegroundColor Green
}

function Build {
    Write-Host "🔨 Building..." -ForegroundColor Cyan
    
    dotnet restore $solutionPath
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    
    dotnet build $solutionPath --configuration $Configuration --no-restore /p:Version=$version
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    
    Write-Host "✅ Build complete" -ForegroundColor Green
}

function Test {
    Write-Host "🧪 Testing..." -ForegroundColor Cyan
    
    dotnet test $solutionPath --configuration $Configuration --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
    
    Write-Host "✅ Tests passed" -ForegroundColor Green
}

function Pack {
    Write-Host "📦 Packing..." -ForegroundColor Cyan
    
    if (-not (Test-Path $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }
    
    # Run dotnet pack and capture output to check for errors
    $output = dotnet pack $projectPath `
        --configuration $Configuration `
        --no-build `
        --output $artifactsDir `
        /p:PackageVersion=$version 2>&1
    
    if ($LASTEXITCODE -ne 0) { 
        Write-Host $output -ForegroundColor Red
        throw "Pack failed" 
    }
    
    $packagePath = Join-Path $artifactsDir "LowlandTech.TinyTools.$version.nupkg"
    
    if (-not (Test-Path $packagePath)) {
        throw "Package file was not created: $packagePath"
    }
    
    Write-Host "✅ Package created: $packagePath" -ForegroundColor Green
    
    # Return only the path, nothing else
    Write-Output $packagePath
}

function Push {
    param([string]$PackagePath)
    
    Write-Host "🚀 Pushing to BaGet..." -ForegroundColor Cyan
    Write-Host "   URL: $script:bagetUrl" -ForegroundColor Gray
    Write-Host "   API Key: $script:bagetApiKey" -ForegroundColor Gray
    
    if (-not (Test-Path $PackagePath)) {
        throw "Package not found: $PackagePath"
    }
    
    # Try pushing with the API key
    dotnet nuget push $PackagePath `
        --source $script:bagetUrl `
        --api-key $script:bagetApiKey `
        --skip-duplicate
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Push failed with API key"
        Write-Warning "BaGet URL: $script:bagetUrl"
        Write-Warning "Trying without API key..."
        
        # Try without API key (some BaGet configurations don't require it)
        dotnet nuget push $PackagePath `
            --source $script:bagetUrl `
            --skip-duplicate
        
        if ($LASTEXITCODE -ne 0) {
            throw "Push failed - check BaGet server configuration and API key"
        }
    }
    
    Write-Host "✅ Pushed to $script:bagetUrl" -ForegroundColor Green
}

# Main execution
try {
    Write-Host "═══════════════════════════════════════" -ForegroundColor Magenta
    Write-Host "  TinyTools Build" -ForegroundColor Magenta
    Write-Host "═══════════════════════════════════════" -ForegroundColor Magenta
    
    # Increment build number (unless -NoBump)
    if (-not $NoBump) {
        $buildNum = Increment-BuildNumber
        Write-Host "📈 Build number: $buildNum" -ForegroundColor Yellow
    } else {
        $buildNum = Get-BuildNumber
        Write-Host "📌 Build number: $buildNum (not incremented)" -ForegroundColor Yellow
    }
    
    # Derive version
    $version = Get-DerivedVersion
    Write-Host "🔢 Version: $version" -ForegroundColor Yellow
    Write-Host "⚙️  Configuration: $Configuration" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════" -ForegroundColor Magenta
    Write-Host ""
    
    switch ($Target) {
        "Clean" {
            Clean
        }
        "Build" {
            Clean
            Build
        }
        "Test" {
            Clean
            Build
            Test
        }
        "Pack" {
            Clean
            Build
            Test
            $package = Pack
        }
        "Push" {
            Clean
            Build
            Test
            $package = Pack
            Push -PackagePath $package
        }
        "CI" {
            Clean
            Build
            Test
            $package = Pack
            Push -PackagePath $package
        }
    }
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════" -ForegroundColor Green
    Write-Host "  ✅ $Target completed successfully!" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════" -ForegroundColor Green
    
} catch {
    Write-Host ""
    Write-Host "═══════════════════════════════════════" -ForegroundColor Red
    Write-Host "  ❌ Build failed: $_" -ForegroundColor Red
    Write-Host "═══════════════════════════════════════" -ForegroundColor Red
    exit 1
}


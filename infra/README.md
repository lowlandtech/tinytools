# Build System

Simple PowerShell-based build system with automatic versioning.

## Quick Start

```powershell
cd infra

# Full CI pipeline (auto-increments build number)
.\build.ps1

# Just build and test (no version bump)
.\build.ps1 -Target Test -NoBump

# Pack without pushing
.\build.ps1 -Target Pack

# Debug build
.\build.ps1 -Configuration Debug
```

## How It Works

1. **BUILDR file** - Stores incrementing build number (starts at 0)
2. **Reads base version** from `src/lib/LowlandTech.TinyTools.csproj` (e.g., `2026.1.0`)
3. **Derives final version** by replacing last segment: `2026.1.{BUILDR}`
   - Base: `2026.1.0` + BUILDR: `5` = **`2026.1.5`**
   - Next build: BUILDR becomes `6` = **`2026.1.6`**

## Targets

| Target | What It Does |
|--------|--------------|
| `CI` (default) | Clean ? Build ? Test ? Pack ? Push to BaGet |
| `Build` | Clean + Build |
| `Test` | Clean + Build + Test |
| `Pack` | Clean + Build + Test + Pack |
| `Push` | Full pipeline with push |
| `Clean` | Remove bin/obj/artifacts |

## Examples

```powershell
# Full CI with auto-increment
.\build.ps1

# Build and test only (don't increment version)
.\build.ps1 -Target Test -NoBump

# Just pack (no push)
.\build.ps1 -Target Pack

# Reset build number
echo 0 > BUILDR
```

## Version Strategy

- **Base version in .csproj**: `2026.1.0` (major.minor.patch)
- **BUILDR**: Auto-incrementing build counter
- **Final version**: `2026.1.{BUILDR}`

To bump major/minor versions:
1. Update `<Version>` in `src/lib/LowlandTech.TinyTools.csproj`
2. Reset BUILDR to 0: `echo 0 > BUILDR`

## Requirements

- .NET 8+ SDK
- BaGet running at http://localhost:5000

## Configure NuGet Source

```bash
dotnet nuget add source http://localhost:5000/v3/index.json --name BaGetLocal
```


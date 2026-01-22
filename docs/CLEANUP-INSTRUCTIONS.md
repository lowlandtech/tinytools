# ?? Repository Cleanup Instructions

## Overview
Clean up the repository root by moving infrastructure files to a dedicated `tools/` folder.

## Quick Start

### Option 1: Automated Cleanup (Recommended)
```powershell
# From repository root, run:
.\CLEANUP-REPO.ps1
```

### Option 2: Manual Cleanup
```powershell
# Create tools directory
New-Item -ItemType Directory -Path "tools" -Force

# Move files
Move-Item "convert-icon.ps1" "tools/"
Move-Item "convert-icon.sh" "tools/"
Move-Item "quick-convert.ps1" "tools/"
Move-Item "ICON.md" "tools/"
```

## Files Being Moved

```
Root ? tools/
??? convert-icon.ps1    ? tools/convert-icon.ps1
??? convert-icon.sh     ? tools/convert-icon.sh
??? quick-convert.ps1   ? tools/quick-convert.ps1
??? ICON.md            ? tools/ICON.md
```

## After Moving Files

### 1. Update .gitignore
Add these entries to your `.gitignore` (they're in `.gitignore-additions.txt`):

```gitignore
# Generated icon
icon.png

# Cleanup scripts (temporary)
CLEANUP-GUIDE.md
CLEANUP-REPO.ps1
.gitignore-additions.txt
```

### 2. Update GitHub Actions
Edit `.github/workflows/tags.yml`:

**Change from:**
```yaml
- name: Convert icon SVG to PNG
  run: |
    if [ -f "icon.svg" ] && [ ! -f "icon.png" ]; then
      convert -background none -resize 128x128 icon.svg icon.png
```

**Change to:**
```yaml
- name: Convert icon SVG to PNG
  run: |
    if [ -f "icon.svg" ] && [ ! -f "icon.png" ]; then
      bash tools/convert-icon.sh
```

### 3. Commit Changes
```powershell
# Stage all changes
git add -A

# Commit
git commit -m "chore: organize infrastructure files into tools/ folder"

# Push
git push origin main
```

## Final Repository Structure

```
tinytools/
??? .github/
?   ??? workflows/
?   ?   ??? main.yml
?   ?   ??? tags.yml
?   ??? ISSUE_TEMPLATE/
?   ?   ??? bug_report.md
?   ?   ??? feature_request.md
?   ??? PULL_REQUEST_TEMPLATE.md
?   ??? dependabot.yml
??? src/
?   ??? lowlandtech.tinytools/
??? test/
?   ??? lowlandtech.tinytools.unittests/
??? samples/
?   ??? README.md
?   ??? HumanizerService.cs
?   ??? CalculatorService.cs
??? tools/                          # ? NEW - Infrastructure folder
?   ??? convert-icon.ps1           # Icon conversion scripts
?   ??? convert-icon.sh
?   ??? quick-convert.ps1
?   ??? ICON.md                    # Icon documentation
??? icon.svg                        # Source icon
??? readme.md                       # Main documentation
??? changelog.md                    # Version history
??? CONTRIBUTING.md                 # Contribution guide
??? .gitignore                      # Ignore generated files
```

## Benefits

? **Cleaner root** - Only essential project files visible  
? **Professional** - Follows OSS best practices  
? **Organized** - Clear separation of concerns  
? **Maintainable** - Easy to find tooling  
? **No pollution** - Generated files ignored

## Verification

After cleanup, your root directory should only contain:
- `.github/` - CI/CD configuration
- `src/` - Source code
- `test/` - Unit tests
- `samples/` - Usage examples
- `tools/` - Infrastructure scripts
- `icon.svg` - Package icon source
- `readme.md`, `changelog.md`, `CONTRIBUTING.md` - Documentation
- `.gitignore`, `.gitattributes` - Git configuration

## Delete After Cleanup

Once you've moved files and committed, delete these temporary files:
```powershell
Remove-Item "CLEANUP-GUIDE.md"
Remove-Item "CLEANUP-REPO.ps1"
Remove-Item ".gitignore-additions.txt"
Remove-Item "icon.png"  # Will be regenerated when needed
```

Or add them to `.gitignore` and let git forget them:
```powershell
git rm --cached CLEANUP-GUIDE.md CLEANUP-REPO.ps1 .gitignore-additions.txt icon.png
git commit -m "chore: remove temporary files"
```

## Questions?

See `tools/ICON.md` for icon conversion documentation after cleanup.

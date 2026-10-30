# Icon Generation

This directory contains the icon source and conversion scripts for the NuGet package.

## Files

- **icon.svg** - Source SVG icon (128x128)
- **icon.png** - Generated PNG icon for NuGet package (created from SVG)

## Quick Start

### Convert SVG to PNG

**Windows (PowerShell):**
```powershell
magick convert -background none -resize 128x128 icon.svg icon.png
```

**Linux/macOS:**
```bash
convert -background none -resize 128x128 icon.svg icon.png
# OR
inkscape -w 128 -h 128 icon.svg -o icon.png
```

## Requirements

You need one of the following tools installed:

### ImageMagick (Recommended)

**Windows:**
- Download from: https://imagemagick.org/script/download.php
- Or install with Chocolatey: `choco install imagemagick`
- **Important:** After installation, restart PowerShell/Terminal to refresh PATH

**macOS:**
```bash
brew install imagemagick
```

**Ubuntu/Debian:**
```bash
sudo apt install imagemagick
```

### Inkscape (Alternative)

**Windows:**
- Download from: https://inkscape.org/
- Or install with Chocolatey: `choco install inkscape`

**macOS:**
```bash
brew install inkscape
```

**Ubuntu/Debian:**
```bash
sudo apt install inkscape
```

## Icon Specifications

- **Size:** 128x128 pixels
- **Format:** PNG with transparency
- **Color space:** RGB
- **Usage:** NuGet package icon

## Troubleshooting

### "Command not found" after installing ImageMagick

1. **Close and reopen** your terminal/PowerShell window
2. Verify installation:
   ```powershell
   magick --version
   ```
3. If still not working, manually add ImageMagick to your PATH:
   - Windows: `C:\Program Files\ImageMagick-[version]`
   - Or run the full path: `"C:\Program Files\ImageMagick-7.x.x\magick.exe" convert ...`

## Icon Design Guidelines

The icon.svg follows these design principles:

- **Minimal:** Simple, recognizable shapes
- **Tiny branding:** Small "TINY" badge in corner
- **Template representation:** `${ }` brackets with dots
- **Code aesthetic:** Horizontal lines suggesting code/template structure
- **Professional:** Dark background with high contrast

## Automated Conversion in CI/CD

The GitHub Actions workflow automatically converts the icon during the build process. However, it's recommended to commit both `icon.svg` and `icon.png` to the repository.

## Updating the Icon

1. Edit `icon.svg` with your preferred SVG editor
2. Run the conversion command above
3. Verify `icon.png` looks correct
4. Commit both files:
   ```bash
   git add icon.svg icon.png
   git commit -m "Update package icon"
   ```

## License

The icon is part of the TinyTemplateEngine project and is licensed under the MIT License.

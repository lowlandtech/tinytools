# Creating Your Social Preview Image (og-image.png)

## Requirements
- **Dimensions**: 1200 x 630 pixels
- **Format**: PNG or JPG
- **File size**: < 5MB (ideally < 300KB)
- **Location**: `site/public/og-image.png`

## Design Recommendations

### Layout
```
???????????????????????????????????????????????????
?                                                 ?
?  [Logo]  FactoryTools                          ?
?                                                 ?
?  Lightweight .NET Template Engine               ?
?                                                 ?
?  • Code Generation                              ?
?  • Data Composition                             ?
?  • Zero Dependencies                            ?
?                                                 ?
?  ? Simpler than Razor                         ?
?  ? Faster than T4                             ?
?                                                 ?
?  tools.lowlandtech.com                          ?
?                                                 ?
???????????????????????????????????????????????????
```

### Design Elements
1. **Background**: Clean gradient or solid color (#0ea5e9 brand blue)
2. **Logo**: Your wrench.svg icon (large, left side)
3. **Title**: "FactoryTools" in large, bold font
4. **Subtitle**: "Lightweight .NET Template Engine"
5. **Key Points**: 2-3 bullet points or icons
6. **CTA/URL**: "tools.lowlandtech.com" at bottom
7. **Code Snippet** (optional): Small example showing ${...} syntax

### Tools for Creation
- **Canva**: canva.com (easy, templates available)
- **Figma**: figma.com (professional, free tier)
- **Photoshop/GIMP**: For full control
- **Placid.app**: Automated OG image generation
- **Cloudinary**: Dynamic OG images

### Quick Canva Template
1. Go to Canva
2. Search "Facebook Post" (1200x630)
3. Use this design:
   - Background: Gradient (#0ea5e9 to #0284c7)
   - Add your wrench icon
   - Title: "FactoryTools" (72pt, Bold)
   - Subtitle: "Lightweight .NET Template Engine" (36pt)
   - Bullet points: (24pt)
     - "? Simpler than Razor"
     - "? Zero Dependencies"  
     - "?? Built for Code Generation"
   - Footer: "tools.lowlandtech.com" (20pt)

### Example Code Snippet (Optional)
```csharp
var template = @"
Hello, ${Name}!
@if (IsAdmin) {
  Welcome, admin!
}";
```

### Color Palette
- **Primary Blue**: #0ea5e9 (Cyan 500)
- **Dark Blue**: #0284c7 (Cyan 600)
- **White**: #ffffff
- **Dark Text**: #1e293b (Slate 800)
- **Light Text**: #f1f5f9 (Slate 100)

### Font Recommendations
- **Title**: Inter Bold, Poppins Bold, or Montserrat Bold
- **Body**: Inter Regular, Open Sans, or Roboto
- **Code**: Fira Code, JetBrains Mono, or Consolas

## Testing Your Image

After creating, test it on:
1. **Facebook Debugger**: https://developers.facebook.com/tools/debug/
2. **Twitter Card Validator**: https://cards-dev.twitter.com/validator
3. **LinkedIn Post Inspector**: https://www.linkedin.com/post-inspector/
4. **OpenGraph.xyz**: https://www.opengraph.xyz/

## Alternative: SVG to PNG Conversion

If you want to use your existing `wrench.svg`:

```bash
# Using ImageMagick
convert -background none -density 300 wrench.svg -resize 200x200 wrench.png

# Or use an online tool
# https://cloudconvert.com/svg-to-png
```

Then compose it with text using Canva or Figma.

## Fallback

If you don't create a custom image yet, update `index.html` to remove the og:image tag:

```html
<!-- Remove or comment out these lines temporarily -->
<!-- <meta property="og:image" content="https://tools.lowlandtech.com/og-image.png" /> -->
<!-- <meta property="twitter:image" content="https://tools.lowlandtech.com/og-image.png" /> -->
```

The page will still be shareable, just without a custom preview image.

## Once Created

1. Save as `site/public/og-image.png`
2. Test the URL: `https://tools.lowlandtech.com/og-image.png`
3. Run through validators above
4. Share on social media to test!

---

**Pro Tip**: Create variations for different platforms:
- `og-image.png` (1200x630 - Twitter/Facebook)
- `og-image-square.png` (1200x1200 - Instagram)
- `og-image-story.png` (1080x1920 - Stories)

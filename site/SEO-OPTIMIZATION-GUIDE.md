# SEO & Discoverability Optimization Guide

This document contains recommendations for improving FactoryTools' discoverability across search engines, AI agents, and developer communities.

## ? Completed Technical Optimizations

### 1. Enhanced Meta Tags (index.html)
- ? Comprehensive meta description with keywords
- ? Open Graph tags for social sharing (Facebook, LinkedIn)
- ? Twitter Card tags for Twitter previews
- ? JSON-LD structured data for search engines
- ? Canonical URL specification
- ? Keywords meta tag for legacy SEO

### 2. Crawler Configuration
- ? robots.txt - Allows all crawlers including AI bots (GPTBot, Claude, ChatGPT, etc.)
- ? sitemap.xml - Main pages listed with priorities
- ? ai-reference.md - Comprehensive guide for AI agents
- ? crawl-guide.md - Site structure guide for crawlers

## ?? Additional Recommendations

### 3. GitHub Repository Optimization

#### Repository Settings (Do on GitHub)
1. **Add Topics/Tags**:
   ```
   .net, csharp, template-engine, code-generation, scaffolding, 
   dotnet, templating, code-generator, dotnet8, dotnet9,
   razor-alternative, t4-alternative, text-generation
   ```

2. **Repository Description**:
   ```
   Lightweight .NET template engine for code generation and data composition. 
   Simpler than Razor, faster than T4. Perfect for scaffolding, configs, and text templating.
   ```

3. **Website URL**: 
   - Set to `https://tools.lowlandtech.com`

4. **Social Preview Image**:
   - Create a 1200x630px image with:
     - FactoryTools logo
     - Tagline: "Lightweight .NET Template Engine"
     - Key features or code snippet
   - Upload to GitHub Social Preview

5. **Enable Discussions**:
   - Turn on GitHub Discussions for community engagement
   - Creates more indexed content

#### README Enhancements
Add to the top of your README.md:

```markdown
## ?? Overview

FactoryTools is a **lightweight .NET template engine** designed for **data composition** and **code generation**—not view rendering.

**Perfect for:**
- ?? Code generation & scaffolding
- ?? Configuration file generation (JSON, YAML, XML)
- ?? Documentation generation (Markdown, README)
- ?? AI prompt composition
- ?? Data transformation & reporting

**Why Choose FactoryTools?**
- ? **Simpler than Razor**: No compilation, no HTML bias
- ? **Lightweight**: Zero dependencies beyond .NET BCL
- ?? **Data-first**: Templates are projections, not views
- ?? **Modern**: Built for .NET 8, 9, and 10
```

### 4. NuGet Package Optimization

Update your `.csproj` file with:

```xml
<PropertyGroup>
  <PackageId>LowlandTech.TinyTools</PackageId>
  <Version>2026.1.0</Version>
  <Authors>LowlandTech</Authors>
  <Company>LowlandTech</Company>
  <Product>FactoryTools</Product>
  
  <!-- SEO Optimized Description -->
  <Description>
    Lightweight .NET template engine for code generation and data composition. 
    Alternative to Razor and T4 for non-view scenarios. Perfect for scaffolding, 
    configuration generation, and text templating. Zero dependencies.
  </Description>
  
  <!-- Rich metadata -->
  <PackageTags>
    template-engine;code-generation;scaffolding;dotnet;csharp;
    templating;text-generation;razor-alternative;t4-alternative;
    code-generator;dotnet8;dotnet9;data-composition
  </PackageTags>
  
  <PackageProjectUrl>https://tools.lowlandtech.com</PackageProjectUrl>
  <RepositoryUrl>https://github.com/lowlandtech/tinytools</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageIcon>icon.png</PackageIcon>
  
  <!-- Release notes -->
  <PackageReleaseNotes>
    See https://github.com/lowlandtech/tinytools/releases
  </PackageReleaseNotes>
</PropertyGroup>

<ItemGroup>
  <None Include="..\..\README.md" Pack="true" PackagePath="\"/>
  <None Include="..\..\icon.png" Pack="true" PackagePath="\"/>
</ItemGroup>
```

### 5. Content Marketing & Backlinks

Create additional content to improve SEO:

1. **Blog Posts** (on dev.to, Medium, or your blog):
   - "Why I Built a Razor Alternative for Code Generation"
   - "Comparing .NET Template Engines: Razor vs T4 vs FactoryTools"
   - "5 Use Cases Where You Shouldn't Use Razor"
   - "Building a Code Generator with FactoryTools"

2. **Stack Overflow Participation**:
   - Answer questions about .NET templating
   - Link to FactoryTools where appropriate
   - Create a tag for `factorytools` or `tinytools`

3. **Reddit/HackerNews**:
   - Share on r/dotnet, r/csharp
   - Post to HackerNews with "Show HN: FactoryTools..."

4. **YouTube/Video Content**:
   - Quick tutorial video
   - Comparison with Razor
   - Live coding session

### 6. Documentation Site Improvements

#### Add These Pages:
1. **Comparison Page** (`/comparison`):
   - Side-by-side: FactoryTools vs Razor vs T4 vs Scriban
   - Performance benchmarks
   - Use case fit matrix

2. **Use Cases Gallery** (`/use-cases`):
   - Real-world examples with code
   - Screenshots of generated output
   - Links to example repositories

3. **Migration Guides**:
   - From Razor/RazorLight
   - From T4 Templates
   - From other template engines

4. **FAQ Page** (`/faq`):
   - Common questions with rich answers
   - Great for SEO (long-tail keywords)

#### Improve Internal Linking:
- Link between related pages
- Add breadcrumbs
- Create a sitemap page for users

### 7. Social Media Presence

1. **Twitter/X**:
   - Regular posts about features
   - Code snippets/tips
   - Use hashtags: #dotnet #csharp #opensource

2. **LinkedIn**:
   - Share blog posts
   - Technical deep-dives
   - Company page for LowlandTech

3. **Dev.to**:
   - Cross-post blog content
   - Tag appropriately

### 8. Community Building

1. **GitHub Sponsors**:
   - Enable sponsorship
   - Shows commitment/sustainability

2. **Contributor Guide**:
   - CONTRIBUTING.md
   - CODE_OF_CONDUCT.md
   - Makes project more approachable

3. **Good First Issues**:
   - Label issues for newcomers
   - Increases contributor base

### 9. Performance & Technical SEO

1. **Site Speed**:
   - Optimize images (use WebP)
   - Minify CSS/JS (Vite does this)
   - Enable compression

2. **Mobile Optimization**:
   - Ensure responsive design
   - Test on mobile devices
   - Check Google Mobile-Friendly Test

3. **Accessibility**:
   - Add alt text to images
   - Proper heading hierarchy (h1, h2, h3)
   - ARIA labels where needed
   - Improves SEO rankings

4. **HTTPS & Security**:
   - Ensure HTTPS is enforced
   - Add security headers

### 10. Analytics & Monitoring

1. **Google Search Console**:
   - Verify site ownership
   - Submit sitemap
   - Monitor search performance
   - Track keyword rankings

2. **Google Analytics** or **Plausible** (privacy-friendly):
   - Track visitor behavior
   - Understand content performance
   - See referral sources

3. **Track Metrics**:
   - NuGet downloads
   - GitHub stars/forks
   - Documentation page views
   - Search rankings for keywords

### 11. Schema Markup Enhancements

Already added JSON-LD for SoftwareApplication. Consider adding:

1. **BreadcrumbList** for navigation
2. **FAQPage** schema for FAQ page
3. **TechArticle** for blog posts/guides
4. **HowTo** schema for tutorials

### 12. Link Building Strategy

1. **Awesome Lists**:
   - Submit to awesome-dotnet
   - Submit to awesome-csharp
   - Create your own curated list

2. **Alternative To Listings**:
   - AlternativeTo.net
   - SourceForge
   - Slant.co

3. **Developer Directories**:
   - NuGet Gallery (already done)
   - .NET Foundation projects
   - Microsoft Tech Community

## ?? Priority Action Items

### High Priority (Do First):
1. ? Enhanced meta tags - DONE
2. ? robots.txt and sitemap.xml - DONE
3. ? AI reference guides - DONE
4. ? Add GitHub topics and social preview
5. ? Update NuGet package metadata
6. ? Set up Google Search Console

### Medium Priority:
7. ? Create comparison page on docs site
8. ? Write first blog post
9. ? Submit to awesome-dotnet
10. ? Add analytics

### Low Priority (Ongoing):
11. ? Regular content creation
12. ? Community engagement
13. ? Social media presence
14. ? Video content

## ?? Keyword Strategy

### Primary Keywords:
- .NET template engine
- C# template engine
- .NET code generation
- Razor alternative
- T4 alternative

### Secondary Keywords:
- lightweight template engine .NET
- C# text templating
- .NET scaffolding tool
- code generator .NET
- data composition .NET

### Long-tail Keywords:
- "simple template engine for .NET without Razor"
- "how to generate code in .NET without T4"
- "lightweight alternative to RazorLight"
- "template engine for configuration files .NET"

## ?? Backlink Opportunities

1. **High Authority**:
   - .NET Blog (guest post)
   - Microsoft Tech Community
   - Scott Hanselman's blog (comment/reach out)

2. **Medium Authority**:
   - Dev.to
   - Medium
   - CodeProject
   - DZone

3. **Community**:
   - Reddit r/dotnet
   - Stack Overflow
   - GitHub Discussions

## ?? Success Metrics

Track these monthly:
- [ ] Google Search Console impressions/clicks
- [ ] NuGet package downloads
- [ ] GitHub stars
- [ ] Documentation page views
- [ ] Keyword rankings (use Google Search Console)
- [ ] Referring domains (backlinks)

## ?? Next Steps

1. Review and implement high-priority items
2. Set up analytics and monitoring
3. Create content calendar
4. Build community engagement plan
5. Monitor and iterate based on data

---

**Questions or need help implementing?**
Open an issue on GitHub or reach out to the maintainers.

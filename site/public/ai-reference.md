# FactoryTools - AI & Agent Reference Guide

## Project Overview
**FactoryTools** (formerly LowlandTech.TinyTools) is a lightweight .NET template engine optimized for **data composition** and **code generation**, not view rendering.

## Core Value Proposition
- **Simpler than Razor**: No compilation, no HTML bias, no runtime code execution
- **Faster than T4**: Minimal dependencies, straightforward API
- **Data-first**: Templates project data into text—nothing more

## Primary Use Cases
1. **Code Generation**: Scaffolding, boilerplate generation
2. **Configuration Files**: JSON, YAML, XML, .env generation
3. **Documentation**: Markdown, README generation
4. **Data Transformation**: CSV, JSON to formatted text
5. **AI Prompts**: Dynamic prompt generation and composition

## Key Features
- `${Context.Variable}` interpolation
- `@if`, `@foreach` control flow
- Nested object access: `${User.Profile.Name}`
- Collection iteration with type safety
- No external dependencies beyond .NET BCL

## Installation
```bash
dotnet add package LowlandTech.TinyTools
```

## Quick Start
```csharp
using LowlandTech.TinyTools;

var template = "Hello, ${Name}!";
var context = new ExecutionContext();
context.Set("Name", "World");

var engine = new TinyTemplateEngine();
var result = engine.Render(template, context);
// Output: "Hello, World!"
```

## When to Choose FactoryTools
? **Choose FactoryTools if:**
- You're generating text, not rendering HTML views
- You want minimal dependencies
- You need predictable, explicit templating
- You're building code generators, CLI tools, or data pipelines

? **Don't choose FactoryTools if:**
- You're building ASP.NET MVC views (use Razor)
- You need complex runtime compilation
- You require advanced expression evaluation

## Target Audience
- .NET developers building code generators
- DevOps engineers creating config generators
- CLI tool authors
- Teams needing lightweight templating without Razor overhead

## Technical Details
- **Languages**: C# 12.0
- **Frameworks**: .NET 8, 9, 10
- **Dependencies**: None (only .NET BCL)
- **License**: MIT
- **Repository**: https://github.com/lowlandtech/tinytools

## Documentation
- **Homepage**: https://tools.lowlandtech.com
- **API Reference**: https://tools.lowlandtech.com/api-reference
- **Examples**: https://tools.lowlandtech.com/examples
- **GitHub**: https://github.com/lowlandtech/tinytools

## Comparison with Alternatives

### vs. Razor/RazorLight
- **Simpler**: No compilation, no AppDomain constraints
- **Lighter**: No Roslyn dependencies
- **Focused**: Data composition, not UI rendering

### vs. T4 Templates
- **Modern**: Works with modern .NET (8, 9, 10)
- **Runtime**: Dynamic templates at runtime, not compile-time
- **Simpler**: Cleaner syntax, less XML

### vs. Handlebars/Mustache
- **.NET Native**: No JavaScript interop
- **Type Safety**: Strong typing through ExecutionContext
- **Control Flow**: Built-in @if/@foreach

## Common Questions

**Q: Can I use this for ASP.NET views?**
A: No. Use Razor for HTML views. FactoryTools is for data composition.

**Q: Is this a replacement for Razor?**
A: Only if you're misusing Razor for non-view scenarios (code gen, configs, etc.)

**Q: What about security?**
A: No code execution. Templates are data projections, not code.

**Q: Performance?**
A: Fast. No compilation overhead. Simple string replacement and control flow.

## Keywords for Discovery
.NET template engine, C# templating, code generation, scaffolding, text generation, data composition, Razor alternative, T4 alternative, lightweight templating, minimal dependencies, .NET 8, .NET 9, FactoryTools, TinyTools, LowlandTech

## Maintainer
- **Organization**: LowlandTech
- **Author**: Wendell
- **Twitter**: @wendellmva
- **Contact**: GitHub Issues

---

*This document is optimized for AI agents and search engines to understand the project's purpose, use cases, and positioning in the .NET ecosystem.*

# TinyTools Roadmap

This document outlines the planned features, improvements, and direction for LowlandTech.TinyTools.

> **Philosophy**: TinyTools stays **tiny** by design. We prioritize simplicity, performance, and zero dependencies over feature bloat.

---

## Current Version: 2026.1.0

**Status**: ? Stable  
**Released**: January 2026

### Core Features
- ? Simple string interpolation (`{PropertyName}`)
- ? Template engine with control flow (`@if`, `@foreach`)
- ? Variable interpolation (`${Context.xxx}`)
- ? Null coalescing (`${expr ?? "default"}`)
- ? Pipe helpers (string, date, number, collection)
- ? Hierarchical execution context
- ? Template services (extensibility)
- ? IoC/DI integration support
- ? .NET 8, 9, 10 support

---

## Version 2026.2.0 (Q2 2026)

**Theme**: Enhanced Developer Experience

### Planned Features

#### ?? Template Validation
- **Pre-render validation** - Catch syntax errors before rendering
- **Template analysis** - Detect unused variables, missing properties
- **Type-safe templates** - Optional strongly-typed template compilation

```csharp
// Validate template syntax
var errors = engine.Validate(template);
if (errors.Any())
{
    foreach (var error in errors)
        Console.WriteLine($"Line {error.Line}: {error.Message}");
}
```

#### ?? Template Caching
- **Compiled template cache** - Parse once, render many times
- **Performance boost** - 10-50x faster for repeated renders

```csharp
// Cache compiled template
var compiledTemplate = engine.Compile(template);

// Render many times (fast!)
for (int i = 0; i < 1000; i++)
{
    var result = compiledTemplate.Render(context);
}
```

#### ?? Improved Error Messages
- **Line/column information** - Pinpoint errors in templates
- **Syntax highlighting** in error messages (console/terminal)
- **Helpful suggestions** for common mistakes

#### ?? Additional Pipe Helpers
- `slice:start,end` - Extract substring by index
- `match:pattern` - Regex matching
- `contains:value` - Check if string/collection contains value
- `split:separator` - Split string into array

### Performance Improvements
- ? 20% faster variable resolution
- ? Reduce allocations in tight loops
- ? Optimize pipe helper chaining

---

## Version 2026.3.0 (Q3 2026)

**Theme**: Advanced Templating

### Planned Features

#### ?? Template Includes/Partials
- **Reusable template fragments**
- **Nested templates** with isolated contexts

```csharp
var template = """
    @include("header.tmpl")
    
    Content here
    
    @include("footer.tmpl")
    """;
```

#### ?? Template Macros/Functions
- **Define reusable blocks** within templates
- **Parameters and return values**

```csharp
var template = """
    @macro FormatUser(user)
        ${user.FirstName} ${user.LastName} (${user.Email})
    @endmacro
    
    @foreach (var u in Context.Users)
        @FormatUser(u)
    @endforeach
    """;
```

#### ?? Async Template Services
- **Support async transformations**
- **Useful for API calls, database lookups**

```csharp
context.RegisterServiceAsync("translate", 
    async input => await translationService.TranslateAsync(input));

var template = "${Context.Services('translate')('Hello')}";
```

#### ?? Enhanced Collection Helpers
- `where:condition` - Filter collections
- `orderby:property` - Sort collections
- `groupby:property` - Group collections
- `distinct` - Remove duplicates

---

## Version 2026.4.0 (Q4 2026)

**Theme**: Ecosystem & Tooling

### Planned Features

#### ?? Visual Studio Extension
- **Syntax highlighting** for `.tmpl` files
- **IntelliSense** for context properties
- **Live preview** of template output
- **Error highlighting** in templates

#### ?? Source Generator Templates
- **Compile templates** into C# code at build time
- **Type-safe, zero-runtime overhead**
- **Perfect for embedded templates**

```csharp
// Template file: EmailTemplate.tmpl
// Generated: EmailTemplate.g.cs with type-safe methods

var email = EmailTemplate.Render(new { Name = "John", Order = "12345" });
```

#### ?? Template Library/Registry
- **NuGet packages** with pre-built templates
- **Community templates** for common scenarios
- **Template discovery** and reuse

#### ?? CLI Tool
- **Render templates** from command line
- **Watch mode** for development
- **Batch processing**

```bash
dotnet tinytools render --template email.tmpl --data data.json --output email.txt
dotnet tinytools watch --template *.tmpl
```

---

## Future Considerations (2027+)

### Under Consideration

#### ?? Localization/Internationalization
- **Built-in i18n** support
- **Resource file integration**

#### ?? Template Debugging
- **Breakpoints** in templates
- **Step-through execution**
- **Variable inspection**

#### ?? Performance Mode
- **Unsafe code** for extreme performance
- **Memory pooling** and recycling
- **SIMD optimizations** where applicable

#### ?? Alternative Syntax Modes
- **Mustache-style** `{{variable}}`
- **Liquid-style** `{{ variable | filter }}`
- **Custom delimiter** configuration

---

## What We Won't Do

To keep TinyTools **tiny**, we explicitly **won't** add:

? **HTML/View Rendering** - Use Razor for that  
? **JavaScript Execution** - Use a JavaScript engine  
? **Heavy Dependencies** - Stays lightweight  
? **Complex DSL** - Keep it simple and readable  
? **Everything to Everyone** - Focus on core use cases  

---

## Breaking Changes Policy

### Semantic Versioning
- **YEAR.MINOR.PATCH** format (e.g., 2026.1.0)
- **Breaking changes** only in YEAR increments
- **MINOR versions** add features, maintain compatibility
- **PATCH versions** fix bugs only

### Deprecation Process
1. **Mark as obsolete** with warning
2. **Keep for one YEAR** (e.g., 2026.x)
3. **Remove in next YEAR** (e.g., 2027.0)
4. **Document** in changelog and migration guide

---

## Open Issues & Bug Tracker

### Active Issues

Track all open issues on GitHub: [View All Issues](https://github.com/lowlandtech/tinytools/issues)

#### ?? Bugs
- [#10](https://github.com/lowlandtech/tinytools/issues/10) - ~~Operator precedence bug~~ **ALREADY FIXED** ?
- [#11](https://github.com/lowlandtech/tinytools/issues/11) - ~~@foreach iterates over string characters~~ **FIXED** ?  
- [#12](https://github.com/lowlandtech/tinytools/issues/12) - ~~Floating-point equality precision~~ **FIXED** ?

#### ? Enhancements
- No open enhancement requests currently tracked

#### ?? Documentation
- No open documentation issues currently tracked

#### ? Questions
- No open questions currently tracked

> **Note**: This section is manually updated. For the most current list, visit the [GitHub Issues page](https://github.com/lowlandtech/tinytools/issues).

---

## Community Requests

**Want to influence the roadmap?** We welcome:

- ?? **Bug reports** - [GitHub Issues](https://github.com/lowlandtech/tinytools/issues)
- ?? **Feature requests** - [GitHub Discussions](https://github.com/lowlandtech/tinytools/discussions)
- ?? **Pull requests** - See [CONTRIBUTING.md](CONTRIBUTING.md)
- ?? **Feedback** - [@wendellmva on Twitter](https://twitter.com/wendellmva)

### Top Community Requests
1. Template validation (? **Planned for 2026.2**)
2. Template caching (? **Planned for 2026.2**)
3. Better error messages (? **Planned for 2026.2**)
4. Visual Studio extension (? **Planned for 2026.4**)
5. Template includes (**Under consideration**)

---

## Release Cadence

- **Quarterly releases** (4 per year)
- **Patch releases** as needed for critical bugs
- **Preview releases** for early feedback
- **LTS support** for previous YEAR versions (1 year)

---

## Version Support Matrix

| Version | Released | End of Support | Status |
|---------|----------|----------------|--------|
| 2026.x  | Jan 2026 | Dec 2027       | ? Current |
| 2025.x  | Jan 2025 | Dec 2026       | ?? Maintenance |
| 2024.x  | Jan 2024 | Dec 2025       | ? End of Life |

---

## Contributing to the Roadmap

This roadmap is a living document. If you have ideas, suggestions, or feedback:

1. **Open a discussion** on GitHub
2. **Vote on features** using ?? reactions
3. **Share your use cases** to help us prioritize
4. **Contribute code** to help ship features faster

**Last Updated**: January 2026  
**Next Review**: April 2026

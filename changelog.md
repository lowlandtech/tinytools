# 2026.10.4
## Language server

* `src/lsp` ships `tinytools-lsp`, an LSP 3.17 server over stdio that publishes template diagnostics,
  completes model members and pipe helper names, and renders a preview on request. It reuses the
  engine, so an editor sees exactly what a build or a render would produce. No editor extension is
  claimed; the server is what an extension hosts.
## Build-time member checking

* The template source generator now resolves every `${Context.*}` path against the declared model
  type through the compilation's semantic model. A typo reports `TTG005` and a nullable dereference
  `TTG006`, both as build errors with the template path and position; `TinyToolsLenient=true` opts
  an item out.
## Strict rendering

* `TemplateRenderOptions` adds an opt-in strict policy: an unknown context key raises
  `TemplateResolutionException` with code `TR001`, an unknown pipe helper `TR002`, and a known key
  that resolved to null `TR003` when `FailOnNull` is set. Each exception carries a one-based line and
  column. The default stays lenient, so existing templates render exactly as before.
## Gates in CI

* CI now runs the behavior-test layout checker and the roadmap validator from `eng/`, vendored with
  provenance from the workspace tooling. Both previously warned and passed because the shared copies
  were not in the checkout; they now fail the build on any finding.
## Documentation baseline

* Removed 12 superseded test-campaign summaries and moved the five remaining guides plus the feature
  backlog into `site/public/guides/`, where they publish with the documentation site. The shared
  roadmap validator now passes, which unblocks the repository's issue tooling.

## Interpolation is locale independent

### Changed

* Every `${...}` interpolation formats numbers and dates with the invariant culture, matching what
  `format` and `number` already did. A template can no longer render `3.14` on one machine and
  `3,14` on another, which mattered most for generated source and data files.
* `round`, `floor` and `ceiling` results now print with a dot decimal separator everywhere.
* The readme's caveat that `round` followed the current culture is removed because it is no longer
  true.
# 2026.10.3

## Included and parameterized templates, asynchronous services, collection helpers

### Added

* `@include("name")` with `RegisterFragment`/`UnregisterFragment`: named in-memory fragments
  compose into a parent template, keep declaration order when nested, render with the
  caller's own context, and refuse a missing or recursive fragment by name instead of
  returning partial output.
* `@macro name(parameters) { body }` definitions and `@name(arguments)` calls: arguments bind
  onto a child context, so a macro body keeps interpolation and control flow, calls cannot
  share state, arity errors name the macro, and self-invocation stops at the recursion limit.
* `TinyTemplateEngine.RenderAsync` with `ExecutionContext.RegisterAsyncService` and
  `@service("name", expression)`: calls are awaited in source order, a fault becomes
  `TemplateServiceException` naming only the service, cancellation propagates before any
  later call, and the synchronous path refuses asynchronous services instead of blocking.
* Collection helpers `where:path==value`, `orderby:path`, `groupby:path` and `distinct`:
  null input is empty, ordering is stable, grouping keeps first-seen key order, and a missing
  member path is refused by name.

### Changed

* `VariableResolver.ResolveExpression` honours pipe helpers, so the same expression works
  outside `${...}` interpolation.
* One version authority: `Directory.Build.props` declares the version, `eng/set-version.ps1`
  updates it together with the readme badge, and the tag must match it.
* The package no longer depends on Moq: the mocking-based logger helper and the legacy
  `WhenTestingFor<T>` base moved to the test infrastructure project.
* `TemplateHelpers.Helpers` is read-only; use `Register`/`Unregister` for process-wide
  helpers or `ExecutionContext.RegisterService` for per-render ones.
# 2026.5.31

## Helper registry is read-only

### Breaking Changes

* `TemplateHelpers.Helpers` is now an `IReadOnlyDictionary<string, Func<object?, string?, object?>>`
  instead of a mutable `Dictionary`.
  - Assignments such as `TemplateHelpers.Helpers["name"] = helper` no longer compile.
  - The registry is a `ConcurrentDictionary` internally, so concurrent renders can no
    longer observe a helper table being mutated mid-render.

### Migration Guide

**Process-wide helper (same behavior as before):**

```csharp
TemplateHelpers.Register("shout", (value, _) => value?.ToString()?.ToUpperInvariant() + "!");
// ...and undo it if it was only needed for one test:
TemplateHelpers.Unregister("shout");
```

**Per-render helper (preferred):** register the transformation on the execution
context so it cannot change what other renders in the process see.

```csharp
var context = new ExecutionContext();
context.RegisterService("shout", input => input?.ToString()?.ToUpperInvariant() + "!");
var text = engine.Render("${Context.Services('shout')('hi')}", context); // HI!
```

Reading the registry is unchanged: `TemplateHelpers.Helpers.ContainsKey("upper")` and
`TemplateHelpers.Helpers.Keys` still work.

## Variable Syntax Update
### Breaking Changes

* Variable interpolation syntax changed from `{var}` to `${var}`
  - Applies to simple interpolation tokens in templates and strings
  - Aligns basic interpolation syntax with the engine's `${...}` expression model
  - Reduces ambiguity between literal braces and interpolation tokens

### Migration Guide

**Before:**
```csharp
var template = "Hello {FirstName} {LastName}!";
```

**After:**
```csharp
var template = "Hello ${FirstName} ${LastName}!";
```

If you are using `ToolContext` or engine expressions, continue using `${Context.Name}` and `${...}` syntax consistently.

---

# 2026.1.3
## ITemplate System - Self-Validating Code Generation Templates

### New Features

* **ITemplate Interface** - Template-first approach to code generation
  - Self-validating templates with embedded test cases
  - Each template carries its own `TestDataJson` and `ExpectedContent`
  - Automatic validation via `Validate()` and `ValidateDetailed()` methods
  - Type-safe with `DataType` property for compile-time safety

* **TemplateBase Abstract Class** - Base implementation for templates
  - Handles data serialization/normalization automatically
  - Template rendering via `TinyTemplateEngine`
  - Detailed validation with diff reporting
  - Cross-platform line ending normalization

* **TemplateRegistry** - Centralized template management
  - `Register()` / `Get()` for manual template registration
  - `DiscoverFromAssembly()` - Auto-discover templates via reflection
  - `DiscoverFromCallingAssembly()` - Discover from calling code
  - `RenderBatch()` - Render multiple templates in one call
  - `ValidateAll()` - Validate all registered templates at once

* **TemplateResult Record** - Rich rendering output
  - `Content` - The rendered template content
  - `Path` - Dynamic output path (supports variable interpolation)
  - `Namespace` - Generated namespace
  - `Metadata` - Optional key-value metadata dictionary

* **TinyTemplateEngine Enhancements**
  - Logical AND operator (`&&`) in conditions
  - Logical OR operator (`||`) in conditions  
  - Ternary expressions: `${Context.Active ? 'Yes' : 'No'}`
  - Short-circuit evaluation for logical operators
  - Parenthesized logical expressions

### Documentation

* New `Self-Validating-Templates.md` comprehensive guide
* Updated `ITemplate-QuickRef.md` with new syntax features
* New ITemplate page on documentation website (`/itemplate`)
* Template syntax reference cards

### Testing

* **Test count: 577 ? 633 tests (+56)**
* Fixed skipped test `ItShouldInterpolateFileExtension`
* `RenderBatch()` method tests (18 tests)
* `DiscoverFromAssembly()` method tests (21 tests)
* `ValidateAll()` edge case tests (10 tests)
* Null deserialization handling tests (3 tests)
* Exception handling in validation tests (4 tests)

### Infrastructure

* NCrunch configuration to exclude site folder
* VS Code launch configuration for site development
* Tasks configuration for npm dev server

### Examples

* `ComponentTemplate` - React component generator
* `CSharpClassTemplate` - C# class with properties/methods generator

---

# 2026.1.1
## Major Release - Template Services & Architecture Improvements

### New Features
* **Template Services Architecture** - Extensible service registration system
  - Simple function-based services with string keys
  - Syntax: `${Context.Services('serviceName')(input)}`
  - Support for both string literals and variable references
  - IoC-friendly design
  
* **Enhanced Variable Resolver** 
  - Method call support with string arguments
  - Chained function calls (e.g., `Services('pluralize')('word')`)
  - Direct delegate invocation
  - Variable reference support in method calls

### Documentation
* Added comprehensive "Why TinyTemplateEngine?" section
* Template Services examples with Humanizer and NCalc
* Real-world usage examples (invoice generation, etc.)
* Sample implementations and best practices

### Architecture
* Introduced `ITemplateService` marker interface
* Added `TemplateServiceFunc` delegate type
* Context service inheritance to child contexts
* Error handling with clear "{service} not registered" messages


### Testing
* **Comprehensive test coverage expansion** - 1,100 ? 1,293 tests (+193)
* Complete test suite for template services
* Multi-language pluralization tests
* Calculator service tests
* Integration tests with real-world scenarios
* **TemplateHelpers test coverage:**
  - `PadLeft` / `PadRight` helper tests
  - `Round` (decimal/float), `Default`, `IfEmpty` tests
  - `Floor`, `Ceiling`, `Count`, `First`, `Last`, `Reverse` tests
  - `Format` helper tests (DateTime, DateTimeOffset, DateOnly, TimeOnly, IFormattable)
  - `Register` custom helper tests
  - `YesNo` and `IsEmpty` conditional helper tests
* **TinyTemplateEngine test coverage:**
  - Negation operator tests (`!Context.Value`)
  - Comparison operator edge cases
  - `IsTruthy` method branch coverage
  - `Compare` and `AreEqual` method tests
* **InterpolationExtensions test coverage:**
  - `InterpolateWithEngine<T>` tests
  - `Interpolate(ToolContext)` tests
  - `Interpolate(List<string>, ToolContext)` tests

### Bug Fixes
* **Fixed delegate property resolution in VariableResolver** - `ToolContext.Get()` method was not being invoked for delegate properties, causing template services to fail silently

### Infrastructure
* Upgraded to .NET 8, 9, and 10 multi-targeting
* Modern GitHub Actions CI/CD workflows
* Cross-platform testing (Ubuntu, Windows, macOS)
* Code coverage integration
* Source Link support for better debugging
* Professional NuGet package metadata

### Dependencies
* Tests now include Humanizer.Core (2.14.1) for examples
* Tests now include NCalc (1.0.0) for calculator examples
* Added Microsoft.SourceLink.GitHub for source debugging

### Breaking Changes
* Removed built-in `pluralize` and `singularize` helpers from core
  - Now available via Template Services pattern
  - Keeps core library minimal and focused

### Migration Guide
**Before (2.0.x):**
```csharp
// Built-in helpers (removed)
var template = "${Context.EntityName | pluralize}";
```

**After (2026.1.1):**
```csharp
// Register as a service
context.RegisterService("pluralize", input => input?.ToString()?.Pluralize());
var template = "${Context.Services('pluralize')(Context.EntityName)}";
```

# 2.0.2
* add debug verification

# 2.0.1
* fix tests

# 2.0.0
* upgrade to .net 7
* upgrade dependencies
* remove support for legacy framework
* move from gitlab to github

# 1.6.1
* fix path bug
* add support for files and collection of templates

# 1.0.5
* make methods testharness virtual
* change namespaces for the unittests

# 1.0.4
* changed namespaces
* add test harness
* replace shouldy with fluentassertions

# 1.0.3
* add push for github
* add release notes
* add license
* remove csproj backup file
* fix bug in test

# 1.0.2: 
* rearrange repository layout
* added ci

# 1.0.1
* added some more tests

# 1.0.0
* created interpolation extension for strings

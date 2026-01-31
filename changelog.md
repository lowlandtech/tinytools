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
  - `Interpolate(ExecutionContext)` tests
  - `Interpolate(List<string>, ExecutionContext)` tests

### Bug Fixes
* **Fixed delegate property resolution in VariableResolver** - `ExecutionContext.Get()` method was not being invoked for delegate properties, causing template services to fail silently

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

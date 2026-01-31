# Self-Validating Templates in TinyTools

## Why Templates Validate Themselves

The `ITemplate` interface has a unique design: **each template carries its own test case**. This isn't just for convenience—it's a deliberate architectural decision that solves real problems in large-scale code generation.

### The Problem

When generating code at scale (100s or 1000s of files), traditional approaches fail:

1. **External tests break easily** - A change to a template breaks tests that live elsewhere
2. **Test coverage gaps** - New templates often ship without tests
3. **AI agents can't test** - When AI creates templates, who writes the tests?
4. **Refactoring fear** - Nobody touches working templates because tests are disconnected

### The Solution: Self-Contained Templates

Every `ITemplate` includes:

```csharp
public interface ITemplate
{
    // Template definition
    string TemplateContent { get; }     // The template itself
    string TemplatePath { get; }        // Output path pattern
    string TemplateNamespace { get; }   // Namespace pattern
    Type DataType { get; }              // Expected input type
    
    // Built-in test case
    string TestDataJson { get; }        // Sample input data
    string ExpectedContent { get; }     // What the output should be
    string? ExpectedPath { get; }       // What the path should be
    string? ExpectedNamespace { get; }  // What the namespace should be
    
    // Rendering & validation
    TemplateResult Render(object data);
    bool Validate();
}
```

## How It Works

### 1. Define Your Template with Test Data

```csharp
public class GreetingTemplate : TemplateBase
{
    public override string TemplatePath => "greetings/${Context.Name}.txt";
    public override string TemplateNamespace => "Greetings";
    
    public override string TemplateContent => @"Hello, ${Context.Name}!
Welcome to ${Context.Company}.
@if (Context.IsVip) {
You are a VIP customer!
}";

    public override Type DataType => typeof(GreetingData);
    
    // Test case: this input...
    public override string TestDataJson => """
        {
            "Name": "Alice",
            "Company": "Acme Corp",
            "IsVip": true
        }
        """;
    
    // ...should produce this output
    public override string ExpectedContent => """
        Hello, Alice!
        Welcome to Acme Corp.
        You are a VIP customer!
        """;
    
    public override string? ExpectedPath => "greetings/Alice.txt";
    public override string? ExpectedNamespace => "Greetings";
}

public record GreetingData
{
    public string Name { get; init; } = "";
    public string Company { get; init; } = "";
    public bool IsVip { get; init; }
}
```

### 2. Template Validates Itself

```csharp
var template = new GreetingTemplate();

// Quick check
if (template.Validate())
{
    Console.WriteLine("? Template works correctly");
}

// Detailed validation
var result = template.ValidateDetailed();
if (!result.IsValid)
{
    Console.WriteLine($"? {result.ErrorMessage}");
    foreach (var (field, (expected, actual)) in result.Differences!)
    {
        Console.WriteLine($"  {field} mismatch:");
        Console.WriteLine($"    Expected: {expected}");
        Console.WriteLine($"    Actual:   {actual}");
    }
}
```

### 3. Registry Validates All Templates at Once

```csharp
var registry = new TemplateRegistry();
registry.DiscoverFromCallingAssembly(); // Auto-discover all templates

var results = registry.ValidateAll();

var passed = results.Count(r => r.Value.IsValid);
var failed = results.Count(r => !r.Value.IsValid);

Console.WriteLine($"Templates: {passed} passed, {failed} failed");

// Show failures
foreach (var (name, validation) in results.Where(r => !r.Value.IsValid))
{
    Console.WriteLine($"  ? {name}: {validation.ErrorMessage}");
}
```

## Benefits of Self-Validation

### 1. **Template + Test = One Unit**

The template and its test case are inseparable. If you copy a template, you copy its test. If you modify a template, you immediately know if it still works.

### 2. **AI Agents Can Create Complete Templates**

When an AI creates a new template, it creates the test case too. No separate test file needed.

```csharp
// AI-generated template is immediately testable
var aiTemplate = new GeneratedButtonTemplate();
if (!aiTemplate.Validate())
{
    Console.WriteLine("AI template has issues - regenerate or fix");
}
```

### 3. **Continuous Validation in CI/CD**

```csharp
// In your test suite
[Fact]
public void AllTemplatesShouldValidate()
{
    var registry = new TemplateRegistry();
    registry.DiscoverFromCallingAssembly();
    
    var results = registry.ValidateAll();
    
    foreach (var (name, validation) in results)
    {
        validation.IsValid.Should().BeTrue(
            $"Template '{name}' failed: {validation.ErrorMessage}");
    }
}
```

### 4. **Detailed Diff on Failure**

When validation fails, you get precise information:

```csharp
var result = template.ValidateDetailed();
// result.ErrorMessage: "Template output does not match expected"
// result.Differences: 
//   Content: (expected: "Hello World", actual: "Hello, World")
//   Path: (expected: "out.txt", actual: "output.txt")
```

### 5. **Graceful Error Handling**

The validation never throws—it catches all exceptions and returns a result:

```csharp
// Even with invalid JSON, you get a proper result
public override string TestDataJson => "{ invalid json }";

var result = template.ValidateDetailed();
// result.IsValid: false
// result.ErrorMessage: "Validation threw exception: ..."
```

## Template Syntax Reference

Templates use the `TinyTemplateEngine` syntax:

### Variables

```csharp
${Context.Name}                    // Simple property
${Context.User.Email}              // Nested property
${Context.Name | upper}            // Pipe helper
${Context.Name | trim | lower}     // Chained pipes
${Context.Value ?? 'default'}      // Null coalescing
```

### Conditionals

```csharp
@if (Context.IsActive) {
Active!
}

@if (Context.Count > 0) {
Has items
} else {
Empty
}

@if (Context.Status == "pending") {
Waiting...
} else if (Context.Status == "approved") {
Approved!
} else {
Unknown
}
```

### Logical Operators

```csharp
@if (Context.IsAdmin && Context.IsActive) {
Admin panel
}

@if (Context.HasEmail || Context.HasPhone) {
Can contact
}

@if (!Context.IsDeleted && (Context.IsPublished || Context.IsPreview)) {
Show content
}
```

### Loops

```csharp
@foreach (var item in Context.Items) {
- ${item.Name}: ${item.Value}
}

@foreach (var user in Context.Users) {
@if (user.IsActive) {
  Active user: ${user.Name}
}
}
```

### Ternary Expressions

```csharp
Status: ${Context.IsActive ? 'Active' : 'Inactive'}
Name: ${Context.Name != null ? Context.Name : 'Anonymous'}
Full: ${Context.HasLast ? Context.First + ' ' + Context.Last : Context.First}
```

### Comments

```csharp
@* This is a comment and won't appear in output *@
@* 
   Multi-line
   comment 
*@
```

## Implementing ITemplate

### Option 1: Extend TemplateBase (Recommended)

```csharp
public class MyTemplate : TemplateBase
{
    public override string TemplatePath => "output/${Context.Name}.cs";
    public override string TemplateNamespace => "MyApp.Generated";
    public override string TemplateContent => "public class ${Context.Name} { }";
    public override Type DataType => typeof(MyData);
    
    public override string TestDataJson => """{ "Name": "Test" }""";
    public override string ExpectedContent => "public class Test { }";
    public override string? ExpectedPath => "output/Test.cs";
    public override string? ExpectedNamespace => "MyApp.Generated";
}
```

### Option 2: Implement ITemplate Directly

For advanced scenarios where you need custom rendering logic:

```csharp
public class CustomTemplate : ITemplate
{
    public string TemplatePath => "custom/output.txt";
    public string TemplateNamespace => "Custom";
    public string TemplateContent => "Custom content";
    public Type DataType => typeof(object);
    
    public string TestDataJson => "{}";
    public string ExpectedContent => "Custom rendered content";
    public string? ExpectedPath => "custom/output.txt";
    public string? ExpectedNamespace => "Custom";
    
    public TemplateResult Render(object data)
    {
        // Your custom rendering logic
        return new TemplateResult
        {
            Content = "Custom rendered content",
            Path = "custom/output.txt",
            Namespace = "Custom"
        };
    }
    
    public bool Validate()
    {
        var result = Render(new object());
        return result.Content == ExpectedContent;
    }
}
```

## Using the Template Registry

### Registration

```csharp
var registry = new TemplateRegistry();

// Manual registration
registry.Register("greeting", new GreetingTemplate());

// Auto-discover from assembly
registry.DiscoverFromAssembly(typeof(GreetingTemplate).Assembly);

// Discover from calling assembly
registry.DiscoverFromCallingAssembly();
```

### Rendering

```csharp
// Single template
var result = registry.Render("greeting", greetingData);
File.WriteAllText(result.Path, result.Content);

// Batch rendering
var batch = new Dictionary<string, object>
{
    ["greeting"] = greetingData,
    ["farewell"] = farewellData
};
var results = registry.RenderBatch(batch);

foreach (var (name, result) in results)
{
    File.WriteAllText(result.Path, result.Content);
}
```

### Validation

```csharp
var results = registry.ValidateAll();

// Summary
var passed = results.Values.Count(r => r.IsValid);
Console.WriteLine($"{passed}/{results.Count} templates valid");

// Details
foreach (var (name, result) in results.Where(r => !r.Value.IsValid))
{
    Console.WriteLine($"? {name}");
    Console.WriteLine($"  Error: {result.ErrorMessage}");
    
    if (result.Differences != null)
    {
        foreach (var (field, diff) in result.Differences)
        {
            Console.WriteLine($"  {field}: expected '{diff.Expected}', got '{diff.Actual}'");
        }
    }
}
```

## Best Practices

### 1. Always Provide ExpectedContent

Without expected content, validation only checks that rendering doesn't throw:

```csharp
// Weak validation - only checks no exception
public override string ExpectedContent => string.Empty;

// Strong validation - checks actual output
public override string ExpectedContent => """
    Expected output here
    """;
```

### 2. Use Raw String Literals for Multi-line Content

```csharp
public override string TemplateContent => """
    public class ${Context.ClassName}
    {
        public ${Context.PropertyType} ${Context.PropertyName} { get; set; }
    }
    """;
```

### 3. Include Path and Namespace Expectations

```csharp
public override string? ExpectedPath => "src/Models/User.cs";
public override string? ExpectedNamespace => "MyApp.Models";
```

### 4. Validate During CI

```csharp
[Fact]
public void AllRegisteredTemplatesShouldValidate()
{
    var registry = new TemplateRegistry();
    registry.DiscoverFromCallingAssembly();
    
    var results = registry.ValidateAll();
    
    results.Values.Should().OnlyContain(
        r => r.IsValid, 
        "All templates should pass validation");
}
```

### 5. Use Typed Data Classes

```csharp
// Good - type-safe
public override Type DataType => typeof(UserData);

// Less safe - no compile-time checks
public override Type DataType => typeof(object);
```

## Summary

Self-validating templates solve the maintainability problem of large-scale code generation:

| Traditional Approach | Self-Validating Templates |
|---------------------|---------------------------|
| Tests live separately | Tests embedded in template |
| Easy to forget tests | Impossible to forget |
| AI can't test | AI generates tests too |
| Manual test updates | Tests update with template |
| Batch validation hard | `registry.ValidateAll()` |

The result: **templates that prove themselves correct**, making large-scale generation safe and maintainable.

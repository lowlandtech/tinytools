# ITemplate Quick Reference

## Creating a Template (3 Steps)

### 1. Define Data Type
```csharp
public record MyData
{
    public string Name { get; init; } = "";
}
```

### 2. Create Template Class
```csharp
public class MyTemplate : TemplateBase
{
    public override string TemplatePath => "output/${Context.Name}.txt";
    public override string TemplateNamespace => "MyApp";
    public override string TemplateContent => "Hello, ${Context.Name}!";
    public override Type DataType => typeof(MyData);
    
    public override string TestDataJson => @"{ ""Name"": ""World"" }";
    public override string ExpectedContent => "Hello, World!";
}
```

### 3. Use It
```csharp
var template = new MyTemplate();
var result = template.Render(new MyData { Name = "Alice" });
// result.Path: output/Alice.txt
// result.Content: Hello, Alice!
```

## Template Syntax

| Feature | Syntax | Example |
|---------|--------|---------|
| Variable | `${Context.Property}` | `${Context.Name}` |
| Nested | `${Context.User.Name}` | `${Context.Profile.Email}` |
| Pipe Helper | `${Context.Name \| helper}` | `${Context.Name \| upper}` |
| Null Coalesce | `${Context.Value ?? 'default'}` | `${Context.Name ?? 'Anonymous'}` |
| Ternary | `${cond ? true : false}` | `${Context.Active ? 'Yes' : 'No'}` |
| Loop | `@foreach (var x in Context.List) { }` | See below |
| Condition | `@if (Context.Flag) { }` | See below |
| Logical AND | `Context.A && Context.B` | `@if (Context.IsAdmin && Context.Active) { }` |
| Logical OR | `Context.A \|\| Context.B` | `@if (Context.HasEmail \|\| Context.HasPhone) { }` |
| Negation | `!Context.Flag` | `@if (!Context.IsDeleted) { }` |
| Comments | `@* comment *@` | `@* This won't appear *@` |

### Pipe Helpers

```csharp
${Context.Name | upper}              // UPPERCASE
${Context.Name | lower}              // lowercase
${Context.Name | capitalize}         // Capitalize
${Context.Name | trim}               // Trim whitespace
${Context.Text | truncate:20}        // Truncate to 20 chars
${Context.Name | trim | upper}       // Chained helpers
```

### Ternary Expression Example
```csharp
Status: ${Context.IsActive ? 'Active' : 'Inactive'}
Name: ${Context.Name != null ? Context.Name : 'Guest'}
```

### Loop Example
```csharp
@foreach (var item in Context.Items) {
  - ${item.Name}: ${item.Value}
}
```

### Condition Example
```csharp
@if (Context.IsActive) {
  Status: Active
} else {
  Status: Inactive
}
```

### Logical Operators Example
```csharp
@if (Context.IsLoggedIn && Context.IsAdmin) {
  Welcome, Admin!
}

@if (Context.HasItems || Context.ShowEmpty) {
  Show container
}
```

## Registry Usage

```csharp
var registry = new TemplateRegistry();


// Register
registry.Register("my-template", new MyTemplate());

// Render
var result = registry.Render("my-template", data);

// Validate all
var results = registry.ValidateAll();

// Auto-discover
registry.DiscoverFromCallingAssembly();
```

## Validation

```csharp
// Simple
if (template.Validate()) { /* ok */ }

// Detailed
var result = template.ValidateDetailed();
if (!result.IsValid)
{
    Console.WriteLine(result.ErrorMessage);
    foreach (var (key, (expected, actual)) in result.Differences)
    {
        Console.WriteLine($"{key}: Expected={expected}, Actual={actual}");
    }
}
```

## Common Patterns

### Dynamic Path
```csharp
public override string TemplatePath => 
    "src/${Context.Module}/${Context.ClassName}.cs";
```

### Multiple Files
```csharp
var main = template.Render(data);
var test = testTemplate.Render(data);
File.WriteAllText(main.Path, main.Content);
File.WriteAllText(test.Path, test.Content);
```

### Batch Generation
```csharp
foreach (var name in names)
{
    var data = CreateData(name);
    var result = registry.Render("template", data);
    File.WriteAllText(result.Path, result.Content);
}
```

### Conditional Path
```csharp
public override string TemplatePath => 
    "${Context.IsTest ? 'tests' : 'src'}/${Context.Name}.cs";
```

## Properties

| Property | Required | Description |
|----------|----------|-------------|
| `TemplatePath` | ? | Output path (supports ${variables}) |
| `TemplateNamespace` | ? | Namespace for generated code |
| `TemplateContent` | ? | Template content |
| `DataType` | ? | Type of data expected |
| `TestDataJson` | Recommended | Test data as JSON |
| `ExpectedContent` | Recommended | Expected output for validation |
| `ExpectedPath` | Optional | Expected path for validation |
| `ExpectedNamespace` | Optional | Expected namespace for validation |

## Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Render(data)` | `TemplateResult` | Renders template with data |
| `Validate()` | `bool` | Validates template (simple) |
| `ValidateDetailed()` | `TemplateValidationResult` | Validates with details |

## TemplateResult

```csharp
public record TemplateResult
{
    public string Content { get; init; }     // Generated content
    public string Path { get; init; }        // Output path
    public string Namespace { get; init; }   // Namespace
    public Dictionary<string, object?>? Metadata { get; init; }
}
```

## Best Practices

? **DO:**
- Use descriptive template names
- Provide test data for validation
- Use strongly-typed data records
- Keep templates simple (projection, not logic)
- Validate templates before production

? **DON'T:**
- Put complex logic in templates
- Use `typeof(object)` as DataType
- Skip test data/validation
- Hardcode values that should be variables

## Examples

See:
- `src/lib/Examples/ComponentTemplate.cs` - React component
- `src/lib/Examples/CSharpClassTemplate.cs` - C# class
- `examples/TemplateSystemExamples.cs` - Runnable examples
- `docs/AI-Agent-Template-Guide.md` - More examples

## Full Documentation

- **User Guide**: `docs/ITemplate-Guide.md`
- **AI Guide**: `docs/AI-Agent-Template-Guide.md`
- **Implementation Summary**: `docs/ITemplate-Implementation-Summary.md`

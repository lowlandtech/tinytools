# ITemplate System - Large-Scale Code Generation

The `ITemplate` system provides a **template-first approach** to code generation, designed for creating hundreds or thousands of files from templates. Perfect for component libraries, scaffolding systems, and AI-assisted code generation.

## Overview

### Key Concepts

- **Template-First**: Templates live on disk and can be easily added by developers or AI agents
- **Self-Validating**: Each template includes test data and expected outputs
- **Type-Safe**: Templates specify their data types for compile-time safety
- **Batch-Ready**: Built for generating 500+ files efficiently
- **Agent-Friendly**: Simple structure for AI agents to create new templates

## Core Components

### 1. `ITemplate` Interface

```csharp
public interface ITemplate
{
    // Template definition
    string TemplatePath { get; }        // Output path (supports ${variables})
    string TemplateNamespace { get; }   // Namespace (supports ${variables})
    string TemplateContent { get; }     // Template content (TinyEngine syntax)
    Type DataType { get; }              // Expected data type
    
    // Self-validation
    string TestDataJson { get; }        // Test data as JSON
    string ExpectedContent { get; }     // Expected output content
    string? ExpectedPath { get; }       // Expected output path
    string? ExpectedNamespace { get; }  // Expected output namespace
    
    // Rendering
    TemplateResult Render(object data);
    bool Validate();
}
```

### 2. `TemplateResult` Record

```csharp
public record TemplateResult
{
    public required string Content { get; init; }
    public required string Path { get; init; }
    public required string Namespace { get; init; }
    public Dictionary<string, object?>? Metadata { get; init; }
}
```

### 3. `TemplateBase` Abstract Class

Base implementation handling:
- Data serialization/normalization
- Template rendering via `TinyTemplateEngine`
- Validation with detailed error reporting

## Quick Start

### Create a Simple Template

```csharp
public class ReadMeTemplate : TemplateBase
{
    public override string TemplatePath => "README-${Context.ProjectName}.md";
    
    public override string TemplateNamespace => "Docs";
    
    public override string TemplateContent => @"
# ${Context.ProjectName}

${Context.Description}

## Features
@foreach (var feature in Context.Features) {
- ${feature}
}

## Installation
\`\`\`bash
${Context.InstallCommand}
\`\`\`
";

    public override Type DataType => typeof(ProjectData);
    
    public override string TestDataJson => @"{
  ""ProjectName"": ""AwesomeLib"",
  ""Description"": ""A fantastic library"",
  ""Features"": [""Fast"", ""Easy"", ""Reliable""],
  ""InstallCommand"": ""npm install awesome-lib""
}";
}

public record ProjectData
{
    public string ProjectName { get; init; } = "";
    public string Description { get; init; } = "";
    public List<string> Features { get; init; } = new();
    public string InstallCommand { get; init; } = "";
}
```

### Render a Template

```csharp
var template = new ReadMeTemplate();
var data = new ProjectData
{
    ProjectName = "MyLib",
    Description = "My awesome library",
    Features = new() { "Fast", "Simple" },
    InstallCommand = "npm install mylib"
};

var result = template.Render(data);

Console.WriteLine($"Path: {result.Path}");
Console.WriteLine($"Content:\n{result.Content}");

// Output:
// Path: README-MyLib.md
// Content:
// # MyLib
// My awesome library
// ...
```

### Validate a Template

```csharp
var template = new ReadMeTemplate();

if (template.Validate())
{
    Console.WriteLine("? Template validation passed!");
}
else
{
    var result = template.ValidateDetailed();
    Console.WriteLine($"? Validation failed: {result.ErrorMessage}");
    
    if (result.Differences != null)
    {
        foreach (var (key, (expected, actual)) in result.Differences)
        {
            Console.WriteLine($"  {key}:");
            Console.WriteLine($"    Expected: {expected}");
            Console.WriteLine($"    Actual: {actual}");
        }
    }
}
```

## Template Registry

The `TemplateRegistry` manages multiple templates and supports auto-discovery:

```csharp
var registry = new TemplateRegistry();

// Manual registration
registry.Register("component", new ComponentTemplate());
registry.Register("class", new CSharpClassTemplate());

// Auto-discovery from assembly
registry.DiscoverFromCallingAssembly();

// Render by name
var result = registry.Render("component", componentData);

// Batch rendering
var batch = new Dictionary<string, object>
{
    ["component"] = componentData1,
    ["class"] = classData1
};
var results = registry.RenderBatch(batch);

// Validate all templates
var validationResults = registry.ValidateAll();
foreach (var (name, validation) in validationResults)
{
    Console.WriteLine($"{name}: {(validation.IsValid ? "?" : "?")}");
}
```

## Large-Scale Generation Example

Generate 500 components in one go:

```csharp
var registry = new TemplateRegistry();
registry.Register("component", new ComponentTemplate());

var components = new[]
{
    "Button", "Input", "Card", "Modal", "Dropdown", // ... 495 more
};

foreach (var componentName in components)
{
    var data = new ComponentData
    {
        ComponentName = componentName,
        Props = new()
        {
            new() { Name = "className", Type = "string" },
            new() { Name = "onClick", Type = "() => void" }
        },
        PropsDestructured = "{ className, onClick }"
    };
    
    var result = registry.Render("component", data);
    
    // Write to disk
    File.WriteAllText(result.Path, result.Content);
}
```

## Best Practices

### 1. Always Include Test Data

```csharp
public override string TestDataJson => @"{
  ""Name"": ""TestValue"",
  ""Count"": 42
}";

public override string ExpectedContent => "Expected output...";
```

### 2. Use Descriptive Template Paths

```csharp
// Good: Clear structure with variables
public override string TemplatePath => 
    "src/${Context.Module}/${Context.ClassName}.cs";

// Avoid: Hardcoded or unclear paths
public override string TemplatePath => "output.txt";
```

### 3. Validate Before Production

```csharp
// In your build/CI process
var registry = new TemplateRegistry();
registry.DiscoverFromAssembly(typeof(MyTemplates).Assembly);

var results = registry.ValidateAll();
if (results.Any(r => !r.Value.IsValid))
{
    throw new Exception("Template validation failed!");
}
```

### 4. Keep Templates Simple

Templates should be **projections** of data, not complex logic:

```csharp
// Good: Simple projection
@foreach (var item in Context.Items) {
  - ${item.Name}
}

// Avoid: Complex logic in templates
@if (Context.Items.Count > 5 && Context.User.IsAdmin && DateTime.Now.Hour < 12) {
  // Too much logic!
}
```

### 5. Use Strongly Typed Data

```csharp
// Good: Type-safe with records
public record ComponentData
{
    public string Name { get; init; } = "";
    public List<PropData> Props { get; init; } = new();
}

public override Type DataType => typeof(ComponentData);

// Avoid: Loosely typed
public override Type DataType => typeof(object);
```

## AI Agent Guidelines

When creating new templates, AI agents should:

1. **Define the data type** as a record
2. **Write the template content** using TinyEngine syntax
3. **Provide test data** as JSON
4. **Specify expected outputs** for validation
5. **Run validation** before committing

Example prompt for AI:

```
Create a template for generating TypeScript interfaces.
- Data should include interface name and properties
- Output path should be `src/types/${InterfaceName}.ts`
- Include test data for a "User" interface
- Validate the template works correctly
```

## Advanced Features

### Custom Metadata

```csharp
var result = template.Render(data);
result = result with 
{
    Metadata = new()
    {
        ["Language"] = "TypeScript",
        ["Framework"] = "React",
        ["Version"] = "18.0"
    }
};
```

### Conditional Path Generation

```csharp
public override string TemplatePath => 
    @"${Context.IsTest ? 'tests' : 'src'}/${Context.ClassName}.cs";
```

### Multi-File Generation

One template can reference others:

```csharp
var componentResult = componentTemplate.Render(data);
var testResult = testTemplate.Render(data);
var storyResult = storyTemplate.Render(data);

// Write all three files
WriteFile(componentResult);
WriteFile(testResult);
WriteFile(storyResult);
```

## Performance Tips

For generating thousands of files:

1. **Batch operations**: Use `RenderBatch()` when possible
2. **Parallel processing**: Render templates in parallel
3. **Stream writes**: Don't hold all results in memory
4. **Incremental generation**: Only regenerate changed templates

```csharp
var results = Parallel.ForEach(dataItems, data =>
{
    var result = template.Render(data);
    File.WriteAllText(result.Path, result.Content);
});
```

## See Also

- `TinyTemplateEngine` - The underlying template engine
- `ITemplateService` - For custom template functions
- `ExecutionContext` - For template data binding

---

**Questions?** Open an issue or check the examples in `src/lib/Examples/`

# Template System Examples

This directory contains examples demonstrating the **ITemplate** system for large-scale code generation.

## Files

### ComponentTemplate.cs
Demonstrates generating React/TypeScript components.

**Use Case**: Generate 500+ UI components for a component library.

**Features**:
- Dynamic file paths with variables
- Props generation with `@foreach`
- TypeScript interface generation
- Full type safety

**Example Output**:
```typescript
export interface ButtonProps {
  label: string;
  onClick: () => void;
}

export const Button: React.FC<ButtonProps> = ({ label, onClick }) => {
  return (
    <div className="button">
      <div>{label}</div>
      <div>{onClick}</div>
    </div>
  );
};
```

### CSharpClassTemplate.cs
Demonstrates generating C# class files.

**Use Case**: Generate model classes, DTOs, entities.

**Features**:
- Namespace-based path generation
- Properties with default values
- Method stubs
- Constructor generation

**Example Output**:
```csharp
namespace MyApp.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    
    public User()
    {
    }
    
    public bool Validate()
    {
        throw new NotImplementedException();
    }
}
```

## Running the Examples

### Option 1: Standalone Examples
```csharp
cd examples
dotnet run TemplateSystemExamples.cs
```

### Option 2: Interactive
```csharp
using LowlandTech.TinyTools.Examples;

// Create template
var template = new ComponentTemplate();

// Create data
var data = new ComponentData
{
    ComponentName = "Card",
    Props = new()
    {
        new() { Name = "title", Type = "string" },
        new() { Name = "content", Type = "string" }
    },
    PropsDestructured = "{ title, content }"
};

// Render
var result = template.Render(data);
Console.WriteLine($"Path: {result.Path}");
Console.WriteLine($"Content:\n{result.Content}");
```

## Creating Your Own Template

Follow this pattern:

```csharp
// 1. Define data type
public record MyData
{
    public string PropertyName { get; init; } = "";
}

// 2. Create template
public class MyTemplate : TemplateBase
{
    public override string TemplatePath => "output/${Context.PropertyName}.ext";
    public override string TemplateNamespace => "MyApp";
    public override string TemplateContent => "Content: ${Context.PropertyName}";
    public override Type DataType => typeof(MyData);
    
    // Test data for validation
    public override string TestDataJson => @"{ ""PropertyName"": ""Test"" }";
    public override string ExpectedContent => "Content: Test";
}

// 3. Use it
var template = new MyTemplate();
var result = template.Render(new MyData { PropertyName = "Value" });
```

## Common Use Cases

### 1. Component Libraries
Generate 500+ UI components with consistent structure:
```csharp
var registry = new TemplateRegistry();
registry.Register("component", new ComponentTemplate());

foreach (var name in componentNames)
{
    var data = CreateComponentData(name);
    var result = registry.Render("component", data);
    File.WriteAllText(result.Path, result.Content);
}
```

### 2. Code Scaffolding
Generate boilerplate code for new features:
```csharp
// Generate controller, model, service, tests
var feature = new FeatureData { Name = "User" };

var controller = controllerTemplate.Render(feature);
var model = modelTemplate.Render(feature);
var service = serviceTemplate.Render(feature);
var tests = testTemplate.Render(feature);
```

### 3. Configuration Files
Generate environment-specific configs:
```csharp
var environments = new[] { "dev", "staging", "prod" };

foreach (var env in environments)
{
    var config = CreateEnvConfig(env);
    var result = envTemplate.Render(config);
    File.WriteAllText(result.Path, result.Content);
}
```

### 4. Documentation
Generate API docs, READMEs, changelogs:
```csharp
var api = AnalyzeApiEndpoints();
var docs = apiDocsTemplate.Render(api);
File.WriteAllText("docs/api.md", docs.Content);
```

### 5. Database Migrations
Generate SQL migration files:
```csharp
var migration = new MigrationData
{
    Name = "CreateUsersTable",
    Table = "Users",
    Columns = GetColumns()
};

var result = migrationTemplate.Render(migration);
File.WriteAllText(result.Path, result.Content);
```

## Validation

All example templates include validation. Run validation:

```csharp
var template = new ComponentTemplate();

if (!template.Validate())
{
    var result = template.ValidateDetailed();
    Console.WriteLine($"Validation failed: {result.ErrorMessage}");
}
```

Validate all templates in registry:

```csharp
var registry = new TemplateRegistry();
registry.Register("component", new ComponentTemplate());
registry.Register("class", new CSharpClassTemplate());

var results = registry.ValidateAll();
foreach (var (name, validation) in results)
{
    Console.WriteLine($"{name}: {(validation.IsValid ? "?" : "?")}");
}
```

## Performance

These examples are optimized for generating hundreds of files:

```
Generating 100 components: ~150ms (1.5ms per file)
Generating 500 components: ~750ms (1.5ms per file)
```

For better performance:
- Use `TemplateRegistry.RenderBatch()` for batch operations
- Consider parallel processing for independent files
- Stream writes to disk instead of buffering

## Further Reading

- **Quick Reference**: `docs/ITemplate-QuickRef.md`
- **User Guide**: `docs/ITemplate-Guide.md`
- **AI Agent Guide**: `docs/AI-Agent-Template-Guide.md`
- **Implementation Details**: `docs/ITemplate-Implementation-Summary.md`

## Need Help?

- Check the docs in `docs/` folder
- Look at test cases in `tests/TemplateSystemTests.cs`
- Open an issue on GitHub

---

**Happy Code Generation!** ??

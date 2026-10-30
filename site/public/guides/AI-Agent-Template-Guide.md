# AI Agent Guide: Creating ITemplate Implementations

This guide shows AI agents how to quickly create new `ITemplate` implementations for code generation.

## Template Creation Pattern

Follow this pattern to create any template:

```csharp
// 1. Define the data structure
public record MyData
{
    public string PropertyName { get; init; } = "";
    // ... add properties as needed
}

// 2. Create the template class
public class MyTemplate : TemplateBase
{
    // 3. Define output path (supports ${variables})
    public override string TemplatePath => "output/${Context.PropertyName}.ext";
    
    // 4. Define namespace
    public override string TemplateNamespace => "MyApp.Generated";
    
    // 5. Write the template content
    public override string TemplateContent => @"
// Your template content here
// Use ${Context.PropertyName} for variables
// Use @if, @foreach for control flow
";

    // 6. Specify the data type
    public override Type DataType => typeof(MyData);
    
    // 7. Provide test data
    public override string TestDataJson => @"{
  ""PropertyName"": ""TestValue""
}";
    
    // 8. Specify expected output (optional but recommended)
    public override string ExpectedContent => @"
// Expected output matching template + test data
";
    
    public override string ExpectedPath => "output/TestValue.ext";
}
```

## Quick Examples

### Example 1: Markdown File Generator

```csharp
public record MarkdownData
{
    public string Title { get; init; } = "";
    public string Author { get; init; } = "";
    public List<string> Sections { get; init; } = new();
}

public class MarkdownTemplate : TemplateBase
{
    public override string TemplatePath => 
        "docs/${Context.Title.Replace(' ', '-').ToLower()}.md";
    
    public override string TemplateNamespace => "Docs";
    
    public override string TemplateContent => @"
# ${Context.Title}

**Author:** ${Context.Author}

---

@foreach (var section in Context.Sections) {
## ${section}

Content for ${section} goes here.

}
";

    public override Type DataType => typeof(MarkdownData);
    
    public override string TestDataJson => @"{
  ""Title"": ""Getting Started"",
  ""Author"": ""John Doe"",
  ""Sections"": [""Installation"", ""Configuration"", ""Usage""]
}";

    public override string ExpectedPath => "docs/getting-started.md";
}
```

### Example 2: API Controller Generator

```csharp
public record ApiControllerData
{
    public string ControllerName { get; init; } = "";
    public string EntityName { get; init; } = "";
    public string RoutePrefix { get; init; } = "";
}

public class ApiControllerTemplate : TemplateBase
{
    public override string TemplatePath => 
        "src/Controllers/${Context.ControllerName}Controller.cs";
    
    public override string TemplateNamespace => "MyApp.Controllers";
    
    public override string TemplateContent => @"
using Microsoft.AspNetCore.Mvc;

namespace ${Context.Namespace};

[ApiController]
[Route(""api/${Context.RoutePrefix}"")]
public class ${Context.ControllerName}Controller : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok();
    }

    [HttpGet(""{id}"")]
    public IActionResult GetById(int id)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult Create([FromBody] ${Context.EntityName} entity)
    {
        return Created();
    }

    [HttpPut(""{id}"")]
    public IActionResult Update(int id, [FromBody] ${Context.EntityName} entity)
    {
        return NoContent();
    }

    [HttpDelete(""{id}"")]
    public IActionResult Delete(int id)
    {
        return NoContent();
    }
}
";

    public override Type DataType => typeof(ApiControllerData);
    
    public override string TestDataJson => @"{
  ""ControllerName"": ""User"",
  ""EntityName"": ""UserDto"",
  ""RoutePrefix"": ""users""
}";

    public override string ExpectedPath => "src/Controllers/UserController.cs";
    
    public override string ExpectedNamespace => "MyApp.Controllers";
}
```

### Example 3: SQL Migration Generator

```csharp
public record MigrationData
{
    public string MigrationName { get; init; } = "";
    public string TableName { get; init; } = "";
    public List<ColumnData> Columns { get; init; } = new();
}

public record ColumnData
{
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public bool Nullable { get; init; }
    public string? DefaultValue { get; init; }
}

public class SqlMigrationTemplate : TemplateBase
{
    public override string TemplatePath => 
        "migrations/${DateTime.UtcNow:yyyyMMddHHmmss}_${Context.MigrationName}.sql";
    
    public override string TemplateNamespace => "Database.Migrations";
    
    public override string TemplateContent => @"
-- Migration: ${Context.MigrationName}
-- Created: ${DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}

CREATE TABLE ${Context.TableName} (
    Id INT PRIMARY KEY IDENTITY(1,1),
@foreach (var col in Context.Columns) {
    ${col.Name} ${col.Type}${!col.Nullable ? ' NOT NULL' : ''}${col.DefaultValue != null ? ' DEFAULT ' + col.DefaultValue : ''},
}
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE INDEX IX_${Context.TableName}_CreatedAt ON ${Context.TableName}(CreatedAt);
";

    public override Type DataType => typeof(MigrationData);
    
    public override string TestDataJson => @"{
  ""MigrationName"": ""CreateUsersTable"",
  ""TableName"": ""Users"",
  ""Columns"": [
    { ""Name"": ""Username"", ""Type"": ""NVARCHAR(100)"", ""Nullable"": false },
    { ""Name"": ""Email"", ""Type"": ""NVARCHAR(255)"", ""Nullable"": false },
    { ""Name"": ""IsActive"", ""Type"": ""BIT"", ""Nullable"": false, ""DefaultValue"": ""1"" }
  ]
}";
}
```

### Example 4: Environment Config Generator

```csharp
public record EnvConfigData
{
    public string Environment { get; init; } = "";
    public Dictionary<string, string> Variables { get; init; } = new();
}

public class EnvConfigTemplate : TemplateBase
{
    public override string TemplatePath => 
        ".env.${Context.Environment.ToLower()}";
    
    public override string TemplateNamespace => "Config";
    
    public override string TemplateContent => @"
# Environment: ${Context.Environment}
# Generated: ${DateTime.UtcNow:yyyy-MM-dd}

@foreach (var kvp in Context.Variables) {
${kvp.Key}=${kvp.Value}
}
";

    public override Type DataType => typeof(EnvConfigData);
    
    public override string TestDataJson => @"{
  ""Environment"": ""Development"",
  ""Variables"": {
    ""DATABASE_URL"": ""localhost:5432"",
    ""API_KEY"": ""dev-key-123"",
    ""DEBUG"": ""true""
  }
}";

    public override string ExpectedPath => ".env.development";
}
```

## Common Patterns

### Pattern 1: List Processing

```csharp
@foreach (var item in Context.Items) {
  - ${item.Name}: ${item.Value}
}
```

### Pattern 2: Conditional Sections

```csharp
@if (Context.IncludeTests) {
  // Test code here
}
```

### Pattern 3: Nested Objects

```csharp
@foreach (var section in Context.Sections) {
  ## ${section.Title}
  @foreach (var item in section.Items) {
    - ${item}
  }
}
```

### Pattern 4: String Transformations

```csharp
${Context.Name.ToLower()}
${Context.Name.Replace(" ", "-")}
${Context.Name.Substring(0, 3)}
```

### Pattern 5: Date/Time Formatting

```csharp
${DateTime.UtcNow:yyyy-MM-dd}
${DateTime.Now:HH:mm:ss}
```

## Validation Best Practices

Always provide test data and expected outputs:

```csharp
public override string TestDataJson => @"{
  ""Name"": ""TestName"",
  ""Value"": 42
}";

// Validate content
public override string ExpectedContent => @"
Expected output exactly matching
the template rendered with test data
";

// Validate path
public override string ExpectedPath => "output/TestName.txt";

// Validate namespace
public override string ExpectedNamespace => "MyApp.Generated";
```

## Error Prevention

### ? DO:

```csharp
// Use clear, descriptive names
public class UserControllerTemplate : TemplateBase

// Provide all test data
public override string TestDataJson => @"{...}";

// Use strongly-typed data
public record UserData { ... }
public override Type DataType => typeof(UserData);

// Include validation
public override string ExpectedContent => "...";
```

### ? DON'T:

```csharp
// Don't use vague names
public class Template1 : TemplateBase

// Don't skip test data
public override string TestDataJson => "{}";

// Don't use object type
public override Type DataType => typeof(object);

// Don't skip validation
// (no ExpectedContent defined)
```

## Testing Your Template

```csharp
// Create instance
var template = new MyTemplate();

// Validate it works
if (!template.Validate())
{
    var result = template.ValidateDetailed();
    Console.WriteLine($"Validation failed: {result.ErrorMessage}");
    // Fix the template!
}

// Test with real data
var data = new MyData { PropertyName = "ActualValue" };
var result = template.Render(data);

Console.WriteLine($"Path: {result.Path}");
Console.WriteLine($"Content:\n{result.Content}");
```

## Quick Reference

| Task | Code |
|------|------|
| Variable | `${Context.PropertyName}` |
| Loop | `@foreach (var x in Context.List) { ... }` |
| Condition | `@if (Context.Flag) { ... }` |
| String method | `${Context.Name.ToUpper()}` |
| Nested property | `${Context.User.Profile.Name}` |
| Date format | `${DateTime.UtcNow:yyyy-MM-dd}` |

## Advanced: Multi-File Templates

Generate multiple related files:

```csharp
public class FeatureTemplate : TemplateBase
{
    // Main file
    public override string TemplatePath => 
        "src/features/${Context.Name}/${Context.Name}.tsx";
    
    public override string TemplateContent => @"...";
    
    // Additional method for related files
    public TemplateResult RenderTest()
    {
        var testContent = @"
import { ${Context.Name} } from './${Context.Name}';
// Test code...
";
        return new TemplateResult
        {
            Content = testContent,
            Path = $"src/features/{Context.Name}/{Context.Name}.test.tsx",
            Namespace = TemplateNamespace
        };
    }
}
```

---

## Summary

To create a new template:

1. ? Define data record
2. ? Extend `TemplateBase`
3. ? Set `TemplatePath`, `TemplateNamespace`, `TemplateContent`
4. ? Specify `DataType`
5. ? Provide `TestDataJson`
6. ? Set `ExpectedContent` (and optionally `ExpectedPath`, `ExpectedNamespace`)
7. ? Run `Validate()` to ensure it works

That's it! Your template is ready for large-scale code generation.

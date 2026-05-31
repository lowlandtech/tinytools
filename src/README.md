# LowlandTech.TinyTools

LowlandTech.TinyTools is a lightweight .NET template engine for text generation, interpolation, and deterministic code generation. It is designed for data composition rather than HTML view rendering.

## Install

```bash
dotnet add package LowlandTech.TinyTools
```

## What it does

- Interpolates values into text templates
- Renders templates with `@if`, `@foreach`, and comments
- Supports pipe helpers and null-coalescing expressions
- Includes file-based template primitives for code generation workflows
- Targets `netstandard2.1`, `net8.0`, `net9.0`, and `net10.0`

## Quick start

### Basic interpolation

```csharp
using LowlandTech.TinyTools;

var template = "Hello ${FirstName} ${LastName}!";
var model = new { FirstName = "Jane", LastName = "Doe" };

var result = template.Interpolate(model);
// Hello Jane Doe!
```

### Dictionary interpolation

```csharp
using LowlandTech.TinyTools;

var template = "Welcome to ${City}, ${Country}!";
var data = new Dictionary<string, string>
{
    ["City"] = "Amsterdam",
    ["Country"] = "Netherlands"
};

var result = template.Interpolate(data);
// Welcome to Amsterdam, Netherlands!
```

### Full template engine

```csharp
using LowlandTech.TinyTools;

var engine = new TinyTemplateEngine();
var context = new ToolContext();
context.Set("Name", "Alice");
context.Set("IsPremium", true);
context.Set("Items", new[] { "Item 1", "Item 2", "Item 3" });

var template = """
Hello ${Context.Name}!

@if (Context.IsPremium) {
Premium access is enabled.
} else {
Upgrade to premium.
}

Items:
@foreach (var item in Context.Items) {
- ${item}
}
""";

var output = engine.Render(template, context);
// Hello Alice!
//
// Premium access is enabled.
//
// Items:
// - Item 1
// - Item 2
// - Item 3
```

## Template syntax

| Feature | Example |
| --- | --- |
| Variable interpolation | `${Context.Model.Name}` |
| Null coalescing | `${Context.Title ?? "Untitled"}` |
| Pipe helpers | `${Context.Name \| upper}` |
| Conditionals | `@if (Context.IsActive) { ... }` |
| Else-if chains | `} else if (Context.Score >= 80) {` |
| Iteration | `@foreach (var item in Context.Items) { ... }` |
| Comments | `@* ignored *@` |

## Common use cases

- Generating Markdown, JSON, YAML, config files, or prompts
- Producing deterministic code generation output
- Keeping presentation-free templates outside MVC or Razor pipelines
- Building reusable file-based templates with validation hooks

## Documentation

- Project site: https://tools.lowlandtech.com
- Source: https://github.com/lowlandtech/tinytools
- Docs folder: https://github.com/lowlandtech/tinytools/tree/develop/docs

## License

MIT
# Interpolation

TinyTemplateEngine supports two interpolation styles:

- Simple `{PropertyName}` string interpolation for quick replacements.
- Engine-backed `${Context...}` interpolation with full template features.

## Simple String Interpolation

Use the `Interpolate` extension for small, tag-based replacements.

```csharp
var template = "Hello {FirstName} {LastName}";
var model = new { FirstName = "John", LastName = "Smith" };

var result = template.Interpolate(model);
// Hello John Smith
```

Dictionary models are supported as well:

```csharp
var template = "Hello {Name}";
var model = new Dictionary<string, string> { ["Name"] = "Jane" };
var result = template.Interpolate(model);
// Hello Jane
```

## Context-Based Interpolation

Use `ExecutionContext` for `${Context...}` expressions and all advanced features.

```csharp
var context = new ExecutionContext();
context.Set("FirstName", "John");
context.Set("LastName", "Smith");

var template = "Hello world, I'm ${Context.FirstName} ${Context.LastName}";
var result = template.Interpolate(context);
// Hello world, I'm John Smith
```

### Model Access

Pass a model through the context or use `InterpolateWithEngine`.

```csharp
var template = "Hello ${Context.Model.FirstName}";
var model = new { FirstName = "John" };

var result = template.InterpolateWithEngine(model);
// Hello John
```

Nested properties work as expected:

```csharp
var template = "City: ${Context.Model.Address.City}";
var model = new { Address = new { City = "Seattle" } };
var result = template.InterpolateWithEngine(model);
// City: Seattle
```

### Null Coalescing

Provide defaults inline using `??`.

```csharp
var template = "Hello, ${Context.Name ?? \"Guest\"}!";
var context = new ExecutionContext();

var result = template.Interpolate(context);
// Hello, Guest!
```

## Lists of Templates

You can interpolate lists while preserving order.

```csharp
var templates = new List<string>
{
    "Hello ${Context.Name}",
    "Welcome to ${Context.City}",
    "Your age is ${Context.Age}"
};

var context = new ExecutionContext();
context.Set("Name", "John");
context.Set("City", "Seattle");
context.Set("Age", 30);

var result = templates.Interpolate(context, engine: null);
// ["Hello John", "Welcome to Seattle", "Your age is 30"]
```

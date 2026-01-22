# Template Service Examples

This folder contains example implementations demonstrating how to extend TinyTemplateEngine with external libraries using Template Services.

## Core Philosophy

**TinyTemplateEngine stays minimal by design.** Advanced features are opt-in through Template Services—simple functions registered with string keys.

## How Template Services Work

```csharp
// 1. Register a service function with a key
context.RegisterService("serviceName", input => {
    // Transform input and return result
    return transformedValue;
});

// 2. Use in templates
var template = "${Context.Services('serviceName')(input)}";
```

## Examples

### Pluralization with Humanizer

**Installation:**
```bash
dotnet add package Humanizer.Core
```

**Usage:**
```csharp
using Humanizer;

var context = new ExecutionContext();

// Register services
context.RegisterService("pluralize", input => 
    input?.ToString()?.Pluralize() ?? "");

context.RegisterService("singularize", input => 
    input?.ToString()?.Singularize() ?? "");

context.RegisterService("titleize", input => 
    input?.ToString()?.Titleize() ?? "");

// Use in template
var template = """
    We have ${Context.Services('pluralize')('customer')}.
    Entity: ${Context.Services('singularize')('categories')}
    Title: ${Context.Services('titleize')('hello world')}
    """;

var result = engine.Render(template, context);
// Output:
// We have customers.
// Entity: category
// Title: Hello World
```

### Calculations with NCalc

**Installation:**
```bash
dotnet add package NCalc
```

**Usage:**
```csharp
using NCalc;

var context = new ExecutionContext();

// Register calculation service
context.RegisterService("calc", input =>
{
    var expr = new Expression(input?.ToString() ?? "0");
    return expr.Evaluate();
});

// With formatting
context.RegisterService("calcf", input =>
{
    var expr = new Expression(input?.ToString() ?? "0");
    var result = expr.Evaluate();
    return result is double d ? d.ToString("F2") : result;
});

// Use in template
var template = """
    Subtotal: $${Context.Services('calcf')('19.99 * 5')}
    Tax: $${Context.Services('calcf')('19.99 * 5 * 0.08')}
    Total: $${Context.Services('calcf')('19.99 * 5 * 1.08')}
    """;

var result = engine.Render(template, context);
// Output:
// Subtotal: $99.95
// Tax: $8.00
// Total: $107.95
```

## Complete Example: Invoice Generation

```csharp
using Humanizer;
using NCalc;

var context = new ExecutionContext();

// Register all needed services
context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
context.RegisterService("calc", input =>
{
    var expr = new Expression(input?.ToString() ?? "0");
    var result = expr.Evaluate();
    return result is double d ? d.ToString("F2") : result;
});

var template = """
    Invoice Summary
    ---------------
    Items: ${Context.Services('calc')('5')} ${Context.Services('pluralize')('widget')}
    Unit Price: $19.99
    
    Subtotal: $${Context.Services('calc')('19.99 * 5')}
    Tax (8%): $${Context.Services('calc')('19.99 * 5 * 0.08')}
    --------------- 
    Total:    $${Context.Services('calc')('19.99 * 5 * 1.08')}
    """;

var result = engine.Render(template, context);
```

## Creating Custom Services

Services are just functions—keep them simple:

```csharp
// String transformation
context.RegisterService("reverse", input => 
    new string(input?.ToString()?.Reverse().ToArray() ?? Array.Empty<char>()));

// Business logic
context.RegisterService("discount", input =>
{
    if (double.TryParse(input?.ToString(), out var amount))
    {
        return amount > 100 ? amount * 0.9 : amount; // 10% off orders > $100
    }
    return input;
});

// Conditional formatting
context.RegisterService("formatPrice", input =>
{
    if (double.TryParse(input?.ToString(), out var price))
    {
        return price.ToString("C2"); // Currency format
    }
    return input;
});
```

## Service Not Found

If you reference a service that isn't registered:

```csharp
var template = "${Context.Services('unknown')('test')}";
// Output: "{unknown not registered}"
```

## Best Practices

1. **Keep services pure** - No side effects, just transformations
2. **Handle nulls gracefully** - Always provide fallback values
3. **Name clearly** - Use descriptive service keys
4. **Stay focused** - Each service should do one thing well
5. **Return appropriate types** - Template engine will handle conversion

## Why This Approach?

? **Simple** - Services are just `Func<object?, object?>`  
? **No boilerplate** - No interfaces to implement  
? **Testable** - Easy to unit test functions  
? **Flexible** - Create any transformation you need  
? **Core stays tiny** - Dependencies only where needed  

## Integration with IoC Containers

You can register services from your DI container:

```csharp
// ASP.NET Core example
public class TemplateServiceRegistrar
{
    private readonly IServiceProvider _serviceProvider;
    
    public TemplateServiceRegistrar(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public ExecutionContext CreateContext()
    {
        var context = new ExecutionContext();
        
        // Register services from DI
        var pluralizer = _serviceProvider.GetService<IPluralizationService>();
        if (pluralizer != null)
        {
            context.RegisterService("pluralize", input => 
                pluralizer.Pluralize(input?.ToString() ?? ""));
        }
        
        return context;
    }
}
```

## Contributing

Have a useful service pattern to share? Submit a PR with:
- The service registration code
- Installation instructions for any packages
- Usage examples
- Real-world use case


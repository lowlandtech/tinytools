# Template Services

Template services let you inject custom transformations into templates without bloating the core engine. They are simple functions registered on `ExecutionContext` and invoked from templates.

## Register a Simple Service

```csharp
var context = new ExecutionContext();
context.RegisterService("pluralize", input => input?.ToString()?.Pluralize());

var template = "We have ${Context.Services('pluralize')('customer')}";
var result = engine.Render(template, context);
// We have customers
```

## Register ITemplateService Implementations

```csharp
public class HumanizerService : ITemplateService
{
    public string Name => "pluralize";

    public object? Transform(object? input)
    {
        return input?.ToString()?.Pluralize();
    }
}

var context = new ExecutionContext();
context.RegisterService(new HumanizerService());
```

## Service Not Found Behavior

If a service is missing, the engine returns a readable placeholder so templates stay debuggable.

```csharp
var template = "${Context.Services('unknown')('test')}";
// Output: "{unknown not registered}"
```

## IoC Integration

For DI patterns and production usage, see `docs/IOC-INTEGRATION.md`.

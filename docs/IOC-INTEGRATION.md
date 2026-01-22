# Template Services - IoC/DI Integration

## Overview

`ITemplateService` supports two usage patterns:

1. **Simple Functions** - Quick inline registrations
2. **IoC/DI Services** - Full dependency injection integration

## ITemplateService Interface

```csharp
public interface ITemplateService
{
    string Name { get; }           // Service key for templates
    object? Transform(object? input);  // Transform function
}

public delegate object? TemplateServiceFunc(object? input);
```

## Usage Patterns

### 1. Simple Function Registration (Basic)

```csharp
var context = new ExecutionContext();

// Inline lambda
context.RegisterService("upper", input => input?.ToString()?.ToUpper());

// Use in template
var result = engine.Render("${Context.Services('upper')('hello')}", context);
// Output: "HELLO"
```

### 2. ITemplateService Implementation (Advanced)

```csharp
public class HumanizerService : ITemplateService
{
    public string Name => "pluralize";
    
    public object? Transform(object? input)
    {
        return input?.ToString()?.Pluralize();
    }
}

// Register
var context = new ExecutionContext();
context.RegisterService(new HumanizerService());

// Use
var result = engine.Render("${Context.Services('pluralize')('customer')}", context);
// Output: "customers"
```

### 3. IoC/DI Integration (Production)

#### ASP.NET Core Startup

```csharp
// Program.cs or Startup.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Register template services
        builder.Services.AddSingleton<ITemplateService, HumanizerService>();
        builder.Services.AddSingleton<ITemplateService, CalculatorService>();
        builder.Services.AddSingleton<ITemplateService, CustomBusinessService>();
        
        // Register template engine
        builder.Services.AddScoped<ITemplateEngine, TinyTemplateEngine>();
        builder.Services.AddScoped<ExecutionContext>();
        
        var app = builder.Build();
        app.Run();
    }
}
```

#### Controller Usage

```csharp
public class TemplateController : ControllerBase
{
    private readonly ITemplateEngine _engine;
    private readonly IEnumerable<ITemplateService> _services;
    
    public TemplateController(
        ITemplateEngine engine,
        IEnumerable<ITemplateService> services)
    {
        _engine = engine;
        _services = services;
    }
    
    [HttpPost("render")]
    public IActionResult Render([FromBody] RenderRequest request)
    {
        var context = new ExecutionContext();
        
        // Register all IoC services
        context.RegisterServices(_services);
        
        // Add template data
        context.Set("Customer", request.Customer);
        context.Set("Order", request.Order);
        
        // Render
        var result = _engine.Render(request.Template, context);
        
        return Ok(new { Result = result });
    }
}
```

## Complete Service Examples

### Humanizer Service with Multiple Functions

```csharp
public class HumanizerService : ITemplateService
{
    private readonly ILogger<HumanizerService> _logger;
    
    public HumanizerService(ILogger<HumanizerService> logger)
    {
        _logger = logger;
    }
    
    public string Name => "humanize";
    
    public object? Transform(object? input)
    {
        _logger.LogDebug("Humanizing: {Input}", input);
        return input?.ToString()?.Humanize();
    }
    
    public string Pluralize(string word) => word.Pluralize();
    public string Singularize(string word) => word.Singularize();
    public string Titleize(string text) => text.Titleize();
    
    // Helper to register all methods as separate services
    public static void RegisterAll(ExecutionContext context, HumanizerService service)
    {
        context.RegisterService("pluralize", input => service.Pluralize(input?.ToString() ?? ""));
        context.RegisterService("singularize", input => service.Singularize(input?.ToString() ?? ""));
        context.RegisterService("titleize", input => service.Titleize(input?.ToString() ?? ""));
        context.RegisterService("humanize", service.Transform);
    }
}
```

### Calculator Service with Dependency Injection

```csharp
public class CalculatorService : ITemplateService
{
    private readonly ILogger<CalculatorService> _logger;
    private readonly IConfiguration _config;
    
    public CalculatorService(
        ILogger<CalculatorService> logger,
        IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }
    
    public string Name => "calc";
    
    public object? Transform(object? input)
    {
        try
        {
            var expr = new NCalc.Expression(input?.ToString() ?? "0");
            var result = expr.Evaluate();
            
            _logger.LogDebug("Calculated: {Expression} = {Result}", input, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Calculation failed: {Expression}", input);
            return 0;
        }
    }
}
```

### Custom Business Service

```csharp
public class PricingService : ITemplateService
{
    private readonly IPricingRepository _repository;
    private readonly IMemoryCache _cache;
    
    public PricingService(
        IPricingRepository repository,
        IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }
    
    public string Name => "pricing";
    
    public object? Transform(object? input)
    {
        // Get product code from input
        var productCode = input?.ToString();
        if (string.IsNullOrEmpty(productCode))
            return null;
        
        // Check cache
        var cacheKey = $"price_{productCode}";
        if (_cache.TryGetValue(cacheKey, out decimal price))
            return price;
        
        // Fetch from repository
        price = _repository.GetPrice(productCode);
        
        // Cache for 5 minutes
        _cache.Set(cacheKey, price, TimeSpan.FromMinutes(5));
        
        return price;
    }
}
```

## Registration Helper Extension

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTemplateServices(
        this IServiceCollection services,
        Action<TemplateServiceOptions>? configure = null)
    {
        var options = new TemplateServiceOptions();
        configure?.Invoke(options);
        
        // Register core services
        services.AddScoped<ITemplateEngine, TinyTemplateEngine>();
        services.AddScoped<ExecutionContext>();
        
        // Register template services
        foreach (var serviceType in options.ServiceTypes)
        {
            services.AddSingleton(typeof(ITemplateService), serviceType);
        }
        
        return services;
    }
}

public class TemplateServiceOptions
{
    internal List<Type> ServiceTypes { get; } = new();
    
    public TemplateServiceOptions AddService<T>() where T : class, ITemplateService
    {
        ServiceTypes.Add(typeof(T));
        return this;
    }
}

// Usage in Program.cs:
builder.Services.AddTemplateServices(options =>
{
    options.AddService<HumanizerService>();
    options.AddService<CalculatorService>();
    options.AddService<PricingService>();
});
```

## Testing with Mocked Services

```csharp
[Fact]
public void TestTemplateWithMockedService()
{
    // Arrange
    var mockService = new Mock<ITemplateService>();
    mockService.Setup(s => s.Name).Returns("test");
    mockService.Setup(s => s.Transform(It.IsAny<object?>()))
               .Returns<object?>(input => $"MOCKED:{input}");
    
    var context = new ExecutionContext();
    context.RegisterService(mockService.Object);
    
    var engine = new TinyTemplateEngine();
    var template = "${Context.Services('test')('hello')}";
    
    // Act
    var result = engine.Render(template, context);
    
    // Assert
    result.Should().Contain("MOCKED:hello");
    mockService.Verify(s => s.Transform("hello"), Times.Once);
}
```

## Best Practices

### 1. Service Naming
- Use lowercase kebab-case: `"pluralize"`, `"calc"`, `"format-price"`
- Be descriptive but concise
- Avoid conflicts with built-in helpers

### 2. Error Handling
```csharp
public object? Transform(object? input)
{
    try
    {
        return DoTransform(input);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Service failed");
        return $"{{error: {ex.Message}}}";  // Or return null
    }
}
```

### 3. Logging
- Log at Debug level for normal operations
- Log at Warning for failures
- Include input/output values for debugging

### 4. Caching
- Cache expensive operations
- Use IMemoryCache or IDistributedCache
- Set appropriate expiration times

### 5. Thread Safety
- Services are singletons by default
- Ensure thread-safe implementations
- Avoid mutable state

## Performance Considerations

### Service Registration
```csharp
// ? Bad: Creating service instances repeatedly
foreach (var template in templates)
{
    var context = new ExecutionContext();
    context.RegisterService(new HumanizerService());  // Creates new instance each time
    engine.Render(template, context);
}

// ? Good: Reuse service instances
var humanizerService = new HumanizerService();
foreach (var template in templates)
{
    var context = new ExecutionContext();
    context.RegisterService(humanizerService);  // Reuses instance
    engine.Render(template, context);
}

// ? Better: Use IoC to manage lifecycle
foreach (var template in templates)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ExecutionContext>();
    var services = scope.ServiceProvider.GetServices<ITemplateService>();
    context.RegisterServices(services);
    engine.Render(template, context);
}
```

## Migration from Simple Functions

```csharp
// Old approach (simple)
context.RegisterService("pluralize", input => input?.ToString()?.Pluralize());

// New approach (IoC)
public class MyTemplateServiceRegistration
{
    public static void Configure(IServiceCollection services)
    {
        services.AddSingleton<ITemplateService, HumanizerService>();
    }
}

// In controller/service
public MyService(IEnumerable<ITemplateService> services)
{
    _services = services;
}

public string Render(string template)
{
    var context = new ExecutionContext();
    context.RegisterServices(_services);
    return _engine.Render(template, context);
}
```

Both approaches work! Use simple functions for quick scripts, use `ITemplateService` for production applications with DI.

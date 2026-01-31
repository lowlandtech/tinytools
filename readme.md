<p align="center">
  <img alt="Version" src="https://img.shields.io/badge/version-2026.1.1-blue.svg?cacheSeconds=2592000" />
    <a href="https://github.com/lowlandtech/tinytools/actions/workflows/main.yml" target="_blank">
        <img alt="Build Status" src="https://github.com/lowlandtech/tinytools/actions/workflows/main.yml/badge.svg?branch=develop" />
  </a>
  <a href="https://github.com/lowlandtech/tinytools/actions/workflows/deploy-docs.yml" target="_blank">
    <img alt="Docs Status" src="https://github.com/lowlandtech/tinytools/actions/workflows/deploy-docs.yml/badge.svg" />
  </a>
  <a href="https://tools.lowlandtech.com" target="_blank">
    <img alt="Documentation" src="https://img.shields.io/badge/documentation-yes-brightgreen.svg" />
  </a>
  <a href="#" target="_blank">
    <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-yellow.svg" />
  </a>
  <a href="https://twitter.com/wendellmva" target="_blank">
    <img alt="Twitter: wendellmva" src="https://img.shields.io/twitter/follow/wendellmva.svg?style=social" />
  </a>
</p>

> A lightweight template engine for .NET with minimal dependencies. Designed for data composition, not view rendering.

### 🏠 [Homepage](https://tools.lowlandtech.com)

---

## Why TinyTemplateEngine?

Most .NET templating solutions—**RazorEngine**, **RazorLight**, and similar—are built around one core assumption:

> _Templates are views, and views are primarily HTML._

That assumption becomes a liability once your problem is **data composition**, not **UI rendering**.

<details>
<summary><strong>The Razor Problem</strong></summary>

Razor excels at MVC-style view rendering, but it introduces friction when used as a **general-purpose templating engine**:

-   **HTML-first design**  
    Razor tightly couples templates to HTML and view concepts, even when the output is not a web page.
    
-   **Compile-time complexity**  
    Runtime compilation, Roslyn dependencies, caching layers, and AppDomain constraints add overhead for problems that don't require them.
    
-   **Control-flow leakage**  
    Logic (`@if`, `@foreach`, helpers) creeps into templates, blurring the line between _data preparation_ and _data projection_.
    
-   **Poor fit for non-visual outputs**  
    Generating JSON, YAML, Markdown, config files, prompts, emails, or documents feels unnatural and verbose.
    

After migrating from **RazorEngine** to **RazorLight**, the core issue remained:  
**the templating model itself was working against the use case.**

</details>

<details>
<summary><strong>A Different Assumption</strong></summary>

**TinyTemplateEngine** starts from a different premise:

> _A template is a projection of data — not a view, not a page, and not an application._

That shift enables a simpler and more predictable model.

</details>

<details>
<summary><strong>What TinyTemplateEngine Optimizes For</strong></summary>

### What TinyTemplateEngine Optimizes For

-   **Data-first templating**  
    Templates exist to merge structured data into text—nothing more.
    
-   **Minimal surface area**  
    No compilation step, no HTML bias, no runtime code execution.
    
-   **Explicit separation of concerns**
    
    -   Data is prepared _outside_ the template
        
    -   Templates only describe **shape and placement**
        
-   **Format-agnostic output**  
    Works equally well for:
    
    -   Text
        
    -   Markdown
        
    -   JSON / YAML
        
    -   Config files
        
    -   Prompts
        
    -   Emails
        
    -   Code generation
        
-   **Predictable behavior**  
    No hidden execution model, no side effects, no magic.
    


</details>

### When to Use TinyTemplateEngine

Use it when:

-   You are **merging data into templates**, not rendering views
    
-   You want templates that are **safe, readable, and boring**
    
-   You care more about **composition and transformation** than UI
    
-   Razor's power is _getting in the way_, not helping
    

If you need a full view engine, Razor is still the right tool.  
If you need a **small, deterministic templating engine**, TinyTemplateEngine exists because that gap was real.

## Install

```sh
dotnet add package LowlandTech.TinyTools
```

## Usage

### Basic String Interpolation

```csharp
// Simple property interpolation with {PropertyName} syntax
var template = "Hello {FirstName} {LastName}!";
var model = new { FirstName = "John", LastName = "Smith" };

var result = template.Interpolate(model);
// Output: "Hello John Smith!"
```

### Dictionary Interpolation

```csharp
var template = "Welcome to {City}, {Country}!";
var data = new Dictionary<string, string>
{
    { "City", "Amsterdam" },
    { "Country", "Netherlands" }
};

var result = template.Interpolate(data);
// Output: "Welcome to Amsterdam, Netherlands!"
```

### TinyTemplateEngine (Advanced)

```csharp
var engine = new TinyTemplateEngine();
var context = new ExecutionContext();
context.Set("Name", "Alice");
context.Set("IsPremium", true);
context.Set("Items", new[] { "Item 1", "Item 2", "Item 3" });

var template = """
    Hello ${Context.Name}!
    
    @if (Context.IsPremium) {
    You have premium access.
    } else {
    Upgrade to premium for more features.
    }
    
    Your items:
    @foreach (var item in Context.Items) {
    - ${item}
    }
    """;

var result = engine.Render(template, context);
```

### Using with Models

```csharp
var engine = new TinyTemplateEngine();
var context = new ExecutionContext
{
    Model = new Customer
    {
        FirstName = "Jane",
        LastName = "Doe",
        Orders = new List<Order>
        {
            new Order { OrderNumber = "ORD-001", Total = "99.99" }
        }
    }
};

var template = """
    Dear ${Context.Model.FirstName} ${Context.Model.LastName},
    
    @foreach (var order in Context.Model.Orders) {
    Order #${order.OrderNumber} - Total: $${order.Total}
    }
    """;

var result = engine.Render(template, context);
```

### Null Coalescing

```csharp
var template = """
    Name: ${Context.Name ?? "Guest"}
    Title: ${Context.Title ?? "No title provided"}
    """;
```

### Conditional Logic with Else-If

```csharp
var template = """
    @if (Context.Score >= 90) {
    Grade: A
    } else if (Context.Score >= 80) {
    Grade: B
    } else if (Context.Score >= 70) {
    Grade: C
    } else {
    Grade: F
    }
    """;
```

## Features

| Feature | Syntax | Example |
|---------|--------|---------|
| **Variable Interpolation** | `${Context.xxx}` | `${Context.Model.Name}` |
| **Null Coalescing** | `${expr ?? "default"}` | `${Context.Title ?? "Untitled"}` |
| **Pipe Helpers** | `${expr \| helper}` | `${Context.Name \| upper}` |
| **Conditionals** | `@if (condition) { }` | `@if (Context.IsActive) { ... }` |
| **Else-If Chains** | `} else if (condition) {` | `} else if (Context.Role == "admin") { ... }` |
| **Negation** | `@if (!condition) { }` | `@if (!Context.IsExpired) { ... }` |
| **Iteration** | `@foreach (var x in collection) { }` | `@foreach (var item in Context.Items) { ... }` |
| **Comments** | `@* comment *@` | `@* TODO: Fix this *@` |
| **Comparison Operators** | `>`, `>=`, `<`, `<=`, `==`, `!=` | `@if (Context.Age >= 21) { ... }` |

## Pipe Helpers

Transform values using the pipe syntax: `${Context.Value | helper}` or `${Context.Value | helper:argument}`

### String Helpers

| Helper | Example | Output |
|--------|---------|--------|
| `upper` | `${Context.Name \| upper}` | `JOHN` |
| `lower` | `${Context.Name \| lower}` | `john` |
| `capitalize` | `${Context.Name \| capitalize}` | `John` |
| `camelcase` | `${Context.Name \| camelcase}` | `firstName` |
| `pascalcase` | `${Context.Name \| pascalcase}` | `FirstName` |
| `trim` | `${Context.Text \| trim}` | `hello` |
| `truncate:N` | `${Context.Desc \| truncate:20}` | `This is a long te...` |
| `replace:old,new` | `${Context.Path \| replace:old,new}` | Replaces text |
| `padleft:N,char` | `${Context.Id \| padleft:5,0}` | `00042` |
| `padright:N,char` | `${Context.Name \| padright:10,.}` | `John......` |

### Date Helpers

| Helper | Example | Output |
|--------|---------|--------|
| `format:pattern` | `${Context.Date \| format:yyyy-MM-dd}` | `2024-06-15` |
| `date` | `${Context.Date \| date}` | `2024-06-15` |
| `date:pattern` | `${Context.Date \| date:dd-MMM-yyyy}` | `15-Jun-2024` |

### Number Helpers

| Helper | Example | Output |
|--------|---------|--------|
| `number` | `${Context.Value \| number}` | `1,234` |
| `format:N2` | `${Context.Price \| format:N2}` | `1,234.57` |
| `format:C` | `${Context.Price \| format:C}` | `$1,234.57` |
| `format:P0` | `${Context.Rate \| format:P0}` | `86%` |
| `round:N` | `${Context.Pi \| round:2}` | `3.14` |
| `floor` | `${Context.Value \| floor}` | `3` |
| `ceiling` | `${Context.Value \| ceiling}` | `4` |

### Collection Helpers

| Helper | Example | Output |
|--------|---------|--------|
| `count` | `${Context.Items \| count}` | `5` |
| `first` | `${Context.Items \| first}` | First item |
| `last` | `${Context.Items \| last}` | Last item |
| `join:separator` | `${Context.Tags \| join:, }` | `a, b, c` |
| `reverse` | `${Context.Word \| reverse}` | `olleH` |

### Conditional Helpers

| Helper | Example | Output |
|--------|---------|--------|
| `default:value` | `${Context.Name \| default:Guest}` | `Guest` if null |
| `ifempty:value` | `${Context.Title \| ifempty:N/A}` | `N/A` if empty |
| `yesno` | `${Context.Active \| yesno}` | `Yes` or `No` |
| `yesno:yes,no` | `${Context.Active \| yesno:On,Off}` | `On` or `Off` |

### Chaining Helpers

Helpers can be chained together:

```csharp
${Context.Name | trim | upper | truncate:20}
${Context.Items | first | upper}
${Context.Date | format:MMMM | upper}
```

## Template Services (Extensibility)

The core library stays **tiny** by design. Complex features like advanced pluralization or calculations are provided through **Template Services**—simple functions you register with string keys.

### How It Works

**Two Ways to Register Services:**

#### 1. Simple Functions (Quick & Easy)
```csharp
// Inline lambda - perfect for simple transformations
context.RegisterService("pluralize", input => input?.ToString()?.Pluralize());

// Use in templates
var template = "We have ${Context.Services('pluralize')('customer')}";
// Output: "We have customers"
```

#### 2. ITemplateService (IoC/DI)
```csharp
// Implement the interface
public class HumanizerService : ITemplateService
{
    public string Name => "pluralize";
    
    public object? Transform(object? input)
    {
        return input?.ToString()?.Pluralize();
    }
}

// Register (simple)
context.RegisterService(new HumanizerService());

// Or with dependency injection (ASP.NET Core)
services.AddSingleton<ITemplateService, HumanizerService>();

// In controller
public MyController(IEnumerable<ITemplateService> services)
{
    var context = new ExecutionContext();
    context.RegisterServices(services);  // Registers all IoC services
}
```

### Example: Pluralization with Humanizer

```csharp
// Install: dotnet add package Humanizer.Core
using Humanizer;

var context = new ExecutionContext();

// Register pluralization service
context.RegisterService("pluralize", input => 
    input?.ToString()?.Pluralize() ?? "");

context.RegisterService("singularize", input => 
    input?.ToString()?.Singularize() ?? "");

// Use in template
var template = "We have 5 ${Context.Services('pluralize')('customer')}.";
var result = engine.Render(template, context);
// Output: "We have 5 customers."
```

### Example: Calculations with NCalc

```csharp
// Install: dotnet add package NCalc
using NCalc;

// Register calculation service
context.RegisterService("calc", input =>
{
    var expr = new Expression(input?.ToString() ?? "0");
    return expr.Evaluate();
});

// Use in template
var template = "Total: $${Context.Services('calc')('19.99 * 5 * 1.08')}";
var result = engine.Render(template, context);
// Output: "Total: $107.9460"
```

### Service Not Found

If a service isn't registered, you get a clear error message:

```csharp
var template = "${Context.Services('unknown')('test')}";
// Output: "{unknown not registered}"
```

### Real-World Example: Invoice Generation

```csharp
// Register services
context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
context.RegisterService("calc", input =>
{
    var expr = new Expression(input?.ToString() ?? "0");
    var result = expr.Evaluate();
    return result is double d ? d.ToString("F2") : result;
});

// Template
var template = """
    Invoice
    -------
    Items: ${Context.Services('calc')('5')} ${Context.Services('pluralize')('widget')}
    Subtotal: $${Context.Services('calc')('19.99 * 5')}
    Tax: $${Context.Services('calc')('19.99 * 5 * 0.08')}
    Total: $${Context.Services('calc')('19.99 * 5 * 1.08')}
    """;

// Output:
// Invoice
// -------
// Items: 5 widgets
// Subtotal: $99.95
// Tax: $8.00
// Total: $107.95
```

### Why This Approach?

✅ **Core stays tiny** - Zero unnecessary dependencies  
✅ **Pay for what you use** - Only add services you need  
✅ **Simple** - Services are just functions, no complex interfaces  
✅ **Testable** - Easy to mock in unit tests  
✅ **Flexible** - Create any transformation you need  
✅ **IoC-friendly** - Full dependency injection support

Services are **simple functions** that transform data—nothing more, nothing less.

### Advanced: IoC/DI Integration

For production applications with ASP.NET Core or other DI containers, see:
**📖 [IoC Integration Guide](docs/IOC-INTEGRATION.md)**

- Implement `ITemplateService` for full DI support
- Inject services from IoC container
- Access dependencies (ILogger, IConfiguration, etc.)
- Production-ready patterns and best practices

## Use Cases

- 📧 **Email/Letter Templates** - Personalized communications
- 💻 **Code Generation** - Generate boilerplate code from models
- ⚙️ **Configuration Files** - Environment-specific configs
- 📄 **Documentation** - Auto-generate docs from metadata
- 🧾 **Invoices/Reports** - Dynamic document generation

## Author

👤 **wendellmva**

* Website: https://lowlandtech.com/wendellmva
* Twitter: [@wendellmva](https://twitter.com/wendellmva)
* Github: [@wendellmva](https://github.com/wendellmva)
* LinkedIn: [@wendellmva](https://linkedin.com/in/wendellmva)

## 🤝 Contributing

Contributions, issues and feature requests are welcome!<br />Feel free to check [issues page](https://github.com/lowlandtech/tinytools/issues). 

## Show your support

Give a ⭐️ if this project helped you!

<a href="https://www.patreon.com/wendellmva">
  <img src="https://c5.patreon.com/external/logo/become_a_patron_button@2x.png" width="160">
</a>

***
_This README was generated with ❤️ by [readme-md-generator](https://github.com/kefranabg/readme-md-generator)_
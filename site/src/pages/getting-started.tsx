import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function GettingStartedPage() {
return (
  <div className="max-w-4xl mx-auto space-y-8">
    <div className="space-y-4">
      <p className="text-lg text-muted-foreground">
        Get up and running with FactoryTools in minutes.
      </p>
    </div>

      <Card>
        <CardHeader>
          <CardTitle>Installation</CardTitle>
          <CardDescription>
            Install the NuGet package using the .NET CLI.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code="dotnet add package LowlandTech.FactoryTools"
            language="bash"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Quick Start - Simple Interpolation</CardTitle>
          <CardDescription>
            The simplest way to use FactoryTools - basic string replacement.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`using LowlandTech.FactoryTools;

// Simple property interpolation
var template = "Hello {FirstName} {LastName}!";
var model = new { FirstName = "John", LastName = "Smith" };

var result = template.Interpolate(model);
// Output: "Hello John Smith!"`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Email Templates</CardTitle>
          <CardDescription>
            A common use case - generating personalized emails.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var template = """
    Hi {CustomerName},

    Thank you for your order #{OrderNumber}.
    Your total is ${TotalAmount}.

    We'll send a confirmation to {Email}.
    """;

var model = new 
{
    CustomerName = "Alice",
    OrderNumber = "12345",
    TotalAmount = "$99.99",
    Email = "alice@example.com"
};

var email = template.Interpolate(model);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>With Template Engine - Dynamic Content</CardTitle>
          <CardDescription>
            Use the template engine for conditional logic and loops.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var engine = new TinyTemplateEngine();
var context = new ExecutionContext();
context.Set("CustomerName", "Bob");
context.Set("IsPremium", true);
context.Set("RecentOrders", new[] { "Book", "Laptop", "Mouse" });

var template = """
    Hello \${Context.CustomerName}!

    @if (Context.IsPremium) {
    ?? Thanks for being a Premium member!
    }

    Your recent orders:
    @foreach (var item in Context.RecentOrders) {
    - \${item}
    }
    """;

var message = engine.Render(template, context);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Supported .NET Versions</CardTitle>
        </CardHeader>
        <CardContent>
          <ul className="list-disc list-inside space-y-1 text-muted-foreground">
            <li>.NET 8.0</li>
            <li>.NET 9.0</li>
            <li>.NET 10.0</li>
          </ul>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Next Steps</CardTitle>
          <CardDescription>
            Explore more features and use cases.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ul className="list-disc list-inside space-y-2 text-muted-foreground">
            <li>
              <a href="/variable-resolver" className="text-primary hover:underline">
                Variable Resolver
              </a> - Learn about interpolation styles and pipe helpers
            </li>
            <li>
              <a href="/template-engine" className="text-primary hover:underline">
                Template Engine
              </a> - Control flow, conditionals, and loops
            </li>
            <li>
              <a href="/examples" className="text-primary hover:underline">
                Real-World Examples
              </a> - Newsletters, invoices, and template services
            </li>
            <li>
              <a href="/code-generation" className="text-primary hover:underline">
                Code Generation
              </a> - Generate C# classes, DTOs, APIs, and source generators
            </li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}

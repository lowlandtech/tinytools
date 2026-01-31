import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

export function ExamplesPage() {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <div className="space-y-4">
        <h1 className="text-3xl font-bold">Real-World Examples</h1>
        <p className="text-lg text-muted-foreground">
          Practical examples demonstrating how to use TinyTools in real scenarios.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Newsletter Template</CardTitle>
          <CardDescription>
            Generate a formatted newsletter with articles and conditional offers.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var context = new ExecutionContext();
context.Set("NewsletterTitle", "Tech Weekly Digest");
context.Set("Edition", "Issue #42");
context.Set("SubscriberName", "Alex");
context.Set("Articles", new List<object>
{
    new { Title = "AI Breakthroughs in 2024", 
          Summary = "Latest developments in artificial intelligence.", 
          Author = "Dr. Smith" },
    new { Title = "Cloud Computing Trends", 
          Summary = "What to expect in cloud infrastructure.", 
          Author = "Jane Miller" },
    new { Title = "Cybersecurity Best Practices", 
          Summary = "Protecting your digital assets.", 
          Author = "Bob Wilson" }
});
context.Set("HasSpecialOffer", true);
context.Set("OfferDescription", "Get 50% off our annual subscription!");

var template = """
    ???????????????????????????
    \${Context.NewsletterTitle} - \${Context.Edition}
    ???????????????????????????

    Hello \${Context.SubscriberName}!

    Here's what's new this week:

    @foreach (var article in Context.Articles) {
    ? \${article.Title}
      \${article.Summary}
      By: \${article.Author}

    }
    @if (Context.HasSpecialOffer) {
    ? SPECIAL OFFER ?
    \${Context.OfferDescription}

    }
    Thank you for reading!

    Unsubscribe: https://example.com/unsubscribe
    """;

var result = engine.Render(template, context);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Invoice Template</CardTitle>
          <CardDescription>
            Create professional invoices with line items and calculations.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var context = new ExecutionContext();
context.Set("InvoiceNumber", "INV-2024-0042");
context.Set("InvoiceDate", "June 15, 2024");
context.Set("DueDate", "July 15, 2024");
context.Set("CompanyName", "TechServices LLC");
context.Set("CompanyAddress", "789 Commerce Street, Chicago, IL 60601");
context.Set("ClientName", "Global Enterprises");
context.Set("ClientContact", "Robert Johnson");
context.Set("ClientAddress", "321 Corporate Blvd, Dallas, TX 75201");
context.Set("LineItems", new List<object>
{
    new { Description = "Web Development Services", 
          Hours = 40, Rate = "150.00", Amount = "6000.00" },
    new { Description = "UI/UX Design", 
          Hours = 20, Rate = "125.00", Amount = "2500.00" },
    new { Description = "Project Management", 
          Hours = 10, Rate = "100.00", Amount = "1000.00" }
});
context.Set("Subtotal", "9500.00");
context.Set("TaxRate", "8%");
context.Set("TaxAmount", "760.00");
context.Set("Total", "10260.00");
context.Set("IsPaid", false);

var template = """
    ???????????????????????????????????????????????????
    ?                        INVOICE                           ?
    ???????????????????????????????????????????????????

    From: \${Context.CompanyName}
          \${Context.CompanyAddress}

    To:   \${Context.ClientName}
          Attn: \${Context.ClientContact}
          \${Context.ClientAddress}

    Invoice #: \${Context.InvoiceNumber}
    Date:      \${Context.InvoiceDate}
    Due Date:  \${Context.DueDate}

    ???????????????????????????????????????????????
    Description                    Hours    Rate      Amount
    ???????????????????????????????????????????????
    @foreach (var item in Context.LineItems) {
    \${item.Description}    \${item.Hours}    $\${item.Rate}    $\${item.Amount}
    }
    ???????????????????????????????????????????????
                                           Subtotal:    $\${Context.Subtotal}
                                           Tax (\${Context.TaxRate}):   $\${Context.TaxAmount}
                                           ?????????????????????????
                                           TOTAL:       $\${Context.Total}

    @if (Context.IsPaid) {
    ? PAID - Thank you for your payment!
    } else {
    Payment due by \${Context.DueDate}. Please remit payment to the address above.
    }

    Thank you for your business!
    """;

var result = engine.Render(template, context);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Template Services</CardTitle>
          <CardDescription>
            Extend TinyTools with custom services like Humanizer and NCalc.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="humanizer">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="humanizer">Humanizer</TabsTrigger>
              <TabsTrigger value="ncalc">NCalc</TabsTrigger>
            </TabsList>
            <TabsContent value="humanizer" className="space-y-4">
              <p className="text-sm text-muted-foreground">
                Use Humanizer to pluralize, singularize, and humanize text.
              </p>
              <CodeBlock
                code={`using Humanizer;

var context = new ExecutionContext();

// Register Humanizer services
context.RegisterService("pluralize", 
    input => input?.ToString()?.Pluralize() ?? "");
context.RegisterService("singularize", 
    input => input?.ToString()?.Singularize() ?? "");

context.Set("EntityName", "customer");
context.Set("CollectionName", "categories");

var template = """
    Entity: \${Context.Services('singularize')(Context.CollectionName)}
    Collection: \${Context.Services('pluralize')(Context.EntityName)}
    
    Irregular plurals:
    - person ? \${Context.Services('pluralize')('person')}
    - child ? \${Context.Services('pluralize')('child')}
    - goose ? \${Context.Services('pluralize')('goose')}
    """;

var result = engine.Render(template, context);
// Output:
// Entity: category
// Collection: customers
// 
// Irregular plurals:
// - person ? people
// - child ? children
// - goose ? geese`}
                language="csharp"
              />
            </TabsContent>
            <TabsContent value="ncalc" className="space-y-4">
              <p className="text-sm text-muted-foreground">
                Use NCalc for runtime mathematical expressions.
              </p>
              <CodeBlock
                code={`using NCalc;

var context = new ExecutionContext();

// Register NCalc calculator service
context.RegisterService("calc", input =>
{
    var expr = new Expression(input?.ToString() ?? "0");
    return expr.Evaluate();
});

var template = """
    Invoice Summary
    ---------------
    Items: \${Context.Services('calc')('5')} widgets
    Subtotal: $\${Context.Services('calc')('19.99 * 5')}
    Tax: $\${Context.Services('calc')('19.99 * 5 * 0.08')}
    Total: $\${Context.Services('calc')('19.99 * 5 * 1.08')}
    """;

var result = engine.Render(template, context);
// Output:
// Invoice Summary
// ---------------
// Items: 5 widgets
// Subtotal: $99.95
// Tax: $8.00
// Total: $107.95`}
                language="csharp"
              />
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Combining Services</CardTitle>
          <CardDescription>
            Use multiple services together in the same template.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var context = new ExecutionContext();

// Register multiple services
context.RegisterService("pluralize", 
    input => input?.ToString()?.Pluralize() ?? "");
context.RegisterService("calc", input =>
{
    var expr = new NCalc.Expression(input?.ToString() ?? "0");
    return expr.Evaluate();
});

var template = """
    Order Summary
    -------------
    We have \${Context.Services('calc')('5 + 3')} \${Context.Services('pluralize')('order')}.
    Total: $\${Context.Services('calc')('8 * 29.99')}
    """;

var result = engine.Render(template, context);
// Output:
// Order Summary
// -------------
// We have 8 orders.
// Total: $239.92`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Custom Template Services</CardTitle>
          <CardDescription>
            Create your own template services by implementing ITemplateService.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`public class HumanizerService : ITemplateService
{
    public string Name => "pluralize";
    
    public object? Transform(object? input)
    {
        return input?.ToString()?.Pluralize();
    }
}

// Register the service
var context = new ExecutionContext();
context.RegisterService(new HumanizerService());

// Use in template
var result = engine.Render(
    "\${Context.Services('pluralize')('customer')}", 
    context);
// Output: customers`}
            language="csharp"
          />
        </CardContent>
      </Card>
    </div>
  );
}

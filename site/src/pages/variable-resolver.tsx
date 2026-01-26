import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function VariableResolverPage() {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <div className="space-y-4">
        <h1 className="text-3xl font-bold">Variable Resolver</h1>
        <p className="text-lg text-muted-foreground">
          The VariableResolver handles <code className="px-1 py-0.5 bg-muted rounded">${"${...}"}</code> interpolation
          with support for nested property access.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Basic Interpolation</CardTitle>
          <CardDescription>
            Access context variables using ${"{Context.VariableName}"} syntax.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`var context = new ExecutionContext();
context.Set("Name", "John");
context.Set("Age", 30);

var template = "Hello, \${Context.Name}! You are \${Context.Age} years old.";
var result = engine.Render(template, context);
// Output: Hello, John! You are 30 years old.`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Nested Property Access</CardTitle>
          <CardDescription>
            Access nested properties using dot notation.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`var context = new ExecutionContext();
context.Set("User", new {
    Name = "Alice",
    Address = new {
        City = "Seattle",
        Country = "USA"
    }
});

var template = "\${Context.User.Name} lives in \${Context.User.Address.City}";
var result = engine.Render(template, context);
// Output: Alice lives in Seattle`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Loop Variables</CardTitle>
          <CardDescription>
            Access loop iteration variables directly.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`var context = new ExecutionContext();
context.Set("Items", new[] {
    new { Name = "Apple", Price = 1.50 },
    new { Name = "Banana", Price = 0.75 }
});

var template = @"
@foreach(var item in Context.Items) {
    \${item.Name}: $\${item.Price}
}";

// Output:
// Apple: $1.50
// Banana: $0.75`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Supported Types</CardTitle>
        </CardHeader>
        <CardContent>
          <ul className="list-disc list-inside space-y-1 text-muted-foreground">
            <li>Strings, numbers, booleans</li>
            <li>Anonymous types</li>
            <li>Classes with public properties</li>
            <li>Dictionaries</li>
            <li>Collections (IEnumerable)</li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}

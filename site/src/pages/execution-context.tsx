import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function ExecutionContextPage() {
return (
  <div className="max-w-4xl mx-auto space-y-8">
    <div className="space-y-4">
      <p className="text-lg text-muted-foreground">
        The ExecutionContext provides a hierarchical key-value store for template data
        with support for parent-child relationships.
      </p>
    </div>

      <Card>
        <CardHeader>
          <CardTitle>Basic Usage</CardTitle>
          <CardDescription>
            Set and get values from the context.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`var context = new ExecutionContext();

// Set values
context.Set("Name", "MyClass");
context.Set("Namespace", "MyApp.Models");
context.Set("IsPublic", true);

// Get values
var name = context.Get<string>("Name");
var isPublic = context.Get<bool>("IsPublic");`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Child Contexts</CardTitle>
          <CardDescription>
            Create child contexts that inherit from parents but can override values.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`var parentContext = new ExecutionContext();
parentContext.Set("AppName", "MyApp");
parentContext.Set("Version", "1.0.0");

// Create a child context
var childContext = parentContext.CreateChild();
childContext.Set("ModuleName", "Auth");

// Child can access parent values
var appName = childContext.Get<string>("AppName"); // "MyApp"
var module = childContext.Get<string>("ModuleName"); // "Auth"

// Child can override parent values
childContext.Set("Version", "2.0.0");
var childVersion = childContext.Get<string>("Version"); // "2.0.0"
var parentVersion = parentContext.Get<string>("Version"); // "1.0.0"`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Use in @foreach Loops</CardTitle>
          <CardDescription>
            Child contexts are automatically created for each loop iteration.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`// The template engine creates child contexts automatically
var template = @"
@foreach(var item in Context.Items) {
    // 'item' is available in a child context
    // Parent context values are still accessible
    Namespace: \${Context.Namespace}
    Item: \${item.Name}
}";

var context = new ExecutionContext();
context.Set("Namespace", "MyApp");
context.Set("Items", new[] {
    new { Name = "First" },
    new { Name = "Second" }
});

var result = engine.Render(template, context);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Key Features</CardTitle>
        </CardHeader>
        <CardContent>
          <ul className="list-disc list-inside space-y-2 text-muted-foreground">
            <li><strong>Hierarchical:</strong> Child contexts inherit from parents</li>
            <li><strong>Isolated:</strong> Child modifications don't affect parents</li>
            <li><strong>Type-safe:</strong> Generic Get&lt;T&gt; method for type-safe retrieval</li>
            <li><strong>Flexible:</strong> Store any object type as values</li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}

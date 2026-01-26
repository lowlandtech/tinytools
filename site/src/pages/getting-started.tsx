import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function GettingStartedPage() {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <div className="space-y-4">
        <h1 className="text-3xl font-bold">Getting Started</h1>
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
          <CardTitle>Basic Usage</CardTitle>
          <CardDescription>
            A simple example to get you started.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`using LowlandTech.FactoryTools;

// Create the template engine
var engine = new TinyTemplateEngine();

// Create an execution context with your data
var context = new ExecutionContext();
context.Set("ClassName", "Person");
context.Set("Properties", new[]
{
    new { Name = "FirstName", Type = "string" },
    new { Name = "LastName", Type = "string" },
    new { Name = "Age", Type = "int" }
});

// Define your template
var template = @"
public class \${Context.ClassName}
{
    @foreach(var prop in Context.Properties) {
    public \${prop.Type} \${prop.Name} { get; set; }
    }
}";

// Render the template
var result = engine.Render(template, context);

Console.WriteLine(result);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Output</CardTitle>
          <CardDescription>
            The rendered output from the example above.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}`}
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
    </div>
  );
}

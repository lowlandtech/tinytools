import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { CheckCircle, FileCode, RefreshCw, Shield, Zap } from "lucide-react";

const simpleTemplateExample = `public class GreetingTemplate : TemplateBase
{
    // Output path - supports \${} interpolation
    public override string TemplatePath => "greetings/\${Context.Name}.txt";
    
    // Namespace for the generated code
    public override string TemplateNamespace => "MyApp.Greetings";
    
    // The template content with TinyTemplateEngine syntax
    public override string TemplateContent => """
        Hello, \${Context.Name}!
        Welcome to \${Context.Company}.
        @if (Context.IsVip) {
        You are a VIP customer!
        }
        """;
    
    // The data type this template expects
    public override Type DataType => typeof(GreetingData);
    
    // Test data for self-validation
    public override string TestDataJson => """
        {
            "Name": "Alice",
            "Company": "Acme Corp",
            "IsVip": true
        }
        """;
    
    // Expected output when rendered with TestDataJson
    public override string ExpectedContent => """
        Hello, Alice!
        Welcome to Acme Corp.
        You are a VIP customer!
        """;
    
    // Expected path (optional)
    public override string? ExpectedPath => "greetings/Alice.txt";
}

public record GreetingData
{
    public string Name { get; init; } = "";
    public string Company { get; init; } = "";
    public bool IsVip { get; init; }
}`;

const renderExample = `// Create the template
var template = new GreetingTemplate();

// Render with custom data
var result = template.Render(new GreetingData
{
    Name = "Bob",
    Company = "TechCorp",
    IsVip = false
});

// Use the result
Console.WriteLine(result.Path);      // "greetings/Bob.txt"
Console.WriteLine(result.Content);   // "Hello, Bob!\\nWelcome to TechCorp."
Console.WriteLine(result.Namespace); // "MyApp.Greetings"

// Write to file
File.WriteAllText(result.Path, result.Content);`;

const validateExample = `var template = new GreetingTemplate();

// Quick validation
if (template.Validate())
{
    Console.WriteLine("? Template is valid");
}

// Detailed validation with error info
var result = template.ValidateDetailed();
if (!result.IsValid)
{
    Console.WriteLine($"? {result.ErrorMessage}");
    
    // Show what didn't match
    foreach (var (field, (expected, actual)) in result.Differences!)
    {
        Console.WriteLine($"  {field}:");
        Console.WriteLine($"    Expected: {expected}");
        Console.WriteLine($"    Actual:   {actual}");
    }
}`;

const registryExample = `var registry = new TemplateRegistry();

// Register templates
registry.Register("greeting", new GreetingTemplate());
registry.Register("email", new EmailTemplate());

// Auto-discover all templates in an assembly
registry.DiscoverFromCallingAssembly();

// Render by name
var result = registry.Render("greeting", greetingData);

// Batch render multiple templates
var batch = new Dictionary<string, object>
{
    ["greeting"] = greetingData,
    ["email"] = emailData
};
var results = registry.RenderBatch(batch);

// Validate all templates at once
var validations = registry.ValidateAll();
foreach (var (name, validation) in validations)
{
    var status = validation.IsValid ? "?" : "?";
    Console.WriteLine($"{status} {name}");
}`;

const componentTemplateExample = `public class ReactComponentTemplate : TemplateBase
{
    public override string TemplatePath => 
        "src/components/\${Context.ComponentName}/\${Context.ComponentName}.tsx";
    
    public override string TemplateNamespace => "App.Components";
    
    public override string TemplateContent => """
        import React from 'react';
        
        interface \${Context.ComponentName}Props {
        @foreach (var prop in Context.Props) {
          \${prop.Name}: \${prop.Type};
        }
        }
        
        export const \${Context.ComponentName}: React.FC<\${Context.ComponentName}Props> = ({
        @foreach (var prop in Context.Props) {
          \${prop.Name},
        }
        }) => {
          return (
            <div className="\${Context.ComponentName | lower}">
              {\/* TODO: Implement \${Context.ComponentName} *\/}
            </div>
          );
        };
        """;
    
    public override Type DataType => typeof(ComponentData);
    
    public override string TestDataJson => """
        {
            "ComponentName": "Button",
            "Props": [
                { "Name": "label", "Type": "string" },
                { "Name": "onClick", "Type": "() => void" }
            ]
        }
        """;
    
    public override string ExpectedContent => """
        import React from 'react';
        
        interface ButtonProps {
          label: string;
          onClick: () => void;
        }
        
        export const Button: React.FC<ButtonProps> = ({
          label,
          onClick,
        }) => {
          return (
            <div className="button">
              {/* TODO: Implement Button */}
            </div>
          );
        };
        """;
}`;

const whySelfValidating = [
  {
    icon: Shield,
    title: "Template + Test = One Unit",
    description: "The template and its test case are inseparable. Modify one, you see if it still works immediately.",
  },
  {
    icon: Zap,
    title: "AI Agent Friendly",
    description: "AI can create complete templates with tests. No separate test file needed.",
  },
  {
    icon: RefreshCw,
    title: "CI/CD Integration",
    description: "Run registry.ValidateAll() in your test suite to validate all templates.",
  },
  {
    icon: FileCode,
    title: "Detailed Diff on Failure",
    description: "When validation fails, you get precise information about what didn't match.",
  },
];

export function ITemplatePage() {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      {/* Header */}
      <div className="space-y-4">
        <h1 className="text-3xl font-bold tracking-tight">ITemplate System</h1>
        <p className="text-lg text-muted-foreground">
          A template-first approach to code generation with <strong>self-validating templates</strong>.
          Each template carries its own test case, making large-scale generation safe and maintainable.
        </p>
      </div>

      {/* Why Self-Validating */}
      <section className="space-y-4">
        <h2 className="text-2xl font-bold">Why Self-Validating Templates?</h2>
        <p className="text-muted-foreground">
          Traditional approaches separate templates from their tests, leading to broken tests,
          missing coverage, and fear of refactoring. ITemplate solves this by embedding the test case
          directly in the template.
        </p>
        <div className="grid gap-4 sm:grid-cols-2">
          {whySelfValidating.map((item) => (
            <Card key={item.title}>
              <CardHeader className="pb-2">
                <div className="flex items-center gap-2">
                  <item.icon className="h-5 w-5 text-primary" />
                  <CardTitle className="text-base">{item.title}</CardTitle>
                </div>
              </CardHeader>
              <CardContent>
                <CardDescription>{item.description}</CardDescription>
              </CardContent>
            </Card>
          ))}
        </div>
      </section>

      {/* Tabs for different topics */}
      <Tabs defaultValue="create" className="space-y-4">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="create">Create</TabsTrigger>
          <TabsTrigger value="render">Render</TabsTrigger>
          <TabsTrigger value="validate">Validate</TabsTrigger>
          <TabsTrigger value="registry">Registry</TabsTrigger>
        </TabsList>

        <TabsContent value="create" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Creating a Template</CardTitle>
              <CardDescription>
                Extend <code className="px-1 py-0.5 bg-muted rounded">TemplateBase</code> and 
                define your template content, expected data type, and test case.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <CodeBlock code={simpleTemplateExample} language="csharp" />
              <div className="flex items-start gap-2 text-sm text-muted-foreground">
                <CheckCircle className="h-4 w-4 mt-0.5 text-green-500" />
                <span>
                  The template includes <code className="px-1 py-0.5 bg-muted rounded">TestDataJson</code> and{" "}
                  <code className="px-1 py-0.5 bg-muted rounded">ExpectedContent</code> for self-validation.
                </span>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="render" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Rendering Templates</CardTitle>
              <CardDescription>
                Call <code className="px-1 py-0.5 bg-muted rounded">Render()</code> with your data
                to get the generated content, path, and namespace.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <CodeBlock code={renderExample} language="csharp" />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="validate" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Validating Templates</CardTitle>
              <CardDescription>
                Templates validate themselves using their embedded test data.
                Get detailed diffs when validation fails.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <CodeBlock code={validateExample} language="csharp" />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="registry" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Template Registry</CardTitle>
              <CardDescription>
                Manage multiple templates, auto-discover from assemblies, and batch render.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <CodeBlock code={registryExample} language="csharp" />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Real-World Example */}
      <section className="space-y-4">
        <h2 className="text-2xl font-bold">Real-World Example: React Component Generator</h2>
        <p className="text-muted-foreground">
          Here's a complete template for generating React TypeScript components with props.
        </p>
        <CodeBlock code={componentTemplateExample} language="csharp" />
      </section>

      {/* Template Syntax Quick Reference */}
      <section className="space-y-4">
        <h2 className="text-2xl font-bold">Template Syntax</h2>
        <div className="grid gap-4 md:grid-cols-2">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-base">Variables</CardTitle>
            </CardHeader>
            <CardContent>
              <code className="text-sm">
                {"${Context.Name}"}<br/>
                {"${Context.User.Email}"}<br/>
                {"${Context.Name | upper}"}<br/>
                {"${Context.Value ?? 'default'}"}
              </code>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-base">Conditionals</CardTitle>
            </CardHeader>
            <CardContent>
              <code className="text-sm">
                {"@if (Context.IsActive) { }"}<br/>
                {"@if (A && B) { }"}<br/>
                {"@if (A || B) { }"}<br/>
                {"} else if (...) { } else { }"}
              </code>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-base">Loops</CardTitle>
            </CardHeader>
            <CardContent>
              <code className="text-sm">
                {"@foreach (var item in Context.Items) {"}<br/>
                {"  ${item.Name}"}<br/>
                {"}"}
              </code>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-base">Ternary & Comments</CardTitle>
            </CardHeader>
            <CardContent>
              <code className="text-sm">
                {"${Context.Active ? 'Yes' : 'No'}"}<br/>
                {"@* This is a comment *@"}
              </code>
            </CardContent>
          </Card>
        </div>
      </section>

      {/* Best Practices */}
      <section className="space-y-4">
        <h2 className="text-2xl font-bold">Best Practices</h2>
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 mt-0.5 text-green-500" />
            <div>
              <p className="font-medium">Always provide ExpectedContent</p>
              <p className="text-sm text-muted-foreground">
                Without it, validation only checks that rendering doesn't throw.
              </p>
            </div>
          </div>
          <div className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 mt-0.5 text-green-500" />
            <div>
              <p className="font-medium">Use typed data classes</p>
              <p className="text-sm text-muted-foreground">
                Define records for your template data to get compile-time safety.
              </p>
            </div>
          </div>
          <div className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 mt-0.5 text-green-500" />
            <div>
              <p className="font-medium">Validate in CI/CD</p>
              <p className="text-sm text-muted-foreground">
                Add a test that calls <code className="px-1 py-0.5 bg-muted rounded">registry.ValidateAll()</code>.
              </p>
            </div>
          </div>
          <div className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 mt-0.5 text-green-500" />
            <div>
              <p className="font-medium">Use raw string literals</p>
              <p className="text-sm text-muted-foreground">
                C# 11+ raw strings (<code className="px-1 py-0.5 bg-muted rounded">"""..."""</code>) make multi-line templates clean.
              </p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}

import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { CodeBlock } from "@/components/code-block";
import { ArrowRight, Zap, Code2, Layers, Settings, Wrench } from "lucide-react";

const features = [
  {
    icon: Code2,
    title: "Template Engine",
    description: "Razor-like syntax with @if, @foreach, and comments support.",
    href: "/template-engine",
  },
  {
    icon: Layers,
    title: "Variable Resolution",
    description: "Powerful ${Context.xxx} interpolation with nested property access.",
    href: "/variable-resolver",
  },
  {
    icon: Settings,
    title: "Execution Context",
    description: "Hierarchical context with parent-child relationships.",
    href: "/execution-context",
  },
  {
    icon: Zap,
    title: "High Performance",
    description: "Optimized for all scenarios with minimal allocations.",
    href: "/getting-started",
  },
];

const helloWorldExample = `// Simple string interpolation
var template = "Hello {FirstName} {LastName}!";
var model = new { FirstName = "John", LastName = "Smith" };

var result = template.Interpolate(model);`;

const helloWorldOutput = `Hello John Smith!`;

const emailExample = `// Email template example
var template = """
    Hi {CustomerName},
    
    Thank you for your order #{OrderNumber}.
    Your total is {Total}.
    
    We'll send confirmation to {Email}.
    """;

var model = new 
{
    CustomerName = "Alice",
    OrderNumber = "12345",
    Total = "$99.99",
    Email = "alice@example.com"
};

var email = template.Interpolate(model);`;

const emailOutput = `Hi Alice,

Thank you for your order #12345.
Your total is $99.99.

We'll send confirmation to alice@example.com.`;

export function HomePage() {
  return (
    <div className="space-y-16">
      {/* Hero Section */}
      <section className="text-center space-y-6 py-12">
        <div className="inline-flex items-center rounded-full border px-4 py-1.5 text-sm font-medium">
          <Wrench className="h-4 w-4 text-primary" />
          <span className="ml-2 text-muted-foreground">
            Tiny tools for .NET code generation
          </span>
        </div>
        <h1 className="text-4xl font-bold tracking-tight sm:text-6xl">
          <span className="text-primary">Tiny</span>Tools
        </h1>
        <p className="mx-auto max-w-2xl text-lg text-muted-foreground">
          A lightweight, powerful template engine and toolset for .NET code generation.
          Perfect for source generators, scaffolding, and dynamic content creation.
        </p>
        <div className="flex items-center justify-center gap-4">
          <Button asChild size="lg">
            <Link to="/getting-started">
              Get Started
              <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
          <Button variant="outline" size="lg" asChild>
            <a
              href="https://github.com/lowlandtech/tinytools"
              target="_blank"
              rel="noopener noreferrer"
            >
              View on GitHub
            </a>
          </Button>
        </div>
      </section>

      {/* Features Grid */}
      <section className="space-y-6">
        <h2 className="text-2xl font-bold text-center">Features</h2>
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {features.map((feature) => (
            <Link key={feature.href} to={feature.href}>
              <Card className="h-full transition-colors hover:border-primary">
                <CardHeader>
                  <feature.icon className="h-10 w-10 text-primary" />
                  <CardTitle className="text-lg">{feature.title}</CardTitle>
                </CardHeader>
                <CardContent>
                  <CardDescription>{feature.description}</CardDescription>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      </section>

      {/* Quick Example */}
      <section className="space-y-6">
        <h2 className="text-2xl font-bold text-center">Quick Start</h2>
        <p className="text-center text-muted-foreground max-w-2xl mx-auto">
          Get started in seconds with simple string interpolation.
        </p>
        
        <div className="max-w-4xl mx-auto space-y-8">
          <div>
            <h3 className="text-lg font-semibold mb-3">Hello World</h3>
            <CodeBlock code={helloWorldExample} language="csharp" />
            <div className="mt-3">
              <p className="text-sm font-medium text-muted-foreground mb-2">Output:</p>
              <div className="bg-muted p-4 rounded-lg">
                <code className="text-sm">{helloWorldOutput}</code>
              </div>
            </div>
          </div>
          
          <div>
            <h3 className="text-lg font-semibold mb-3">Email Template</h3>
            <CodeBlock code={emailExample} language="csharp" />
            <div className="mt-3">
              <p className="text-sm font-medium text-muted-foreground mb-2">Output:</p>
              <div className="bg-muted p-4 rounded-lg">
                <pre className="text-sm whitespace-pre-wrap">{emailOutput}</pre>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Installation */}
      <section className="space-y-6">
        <h2 className="text-2xl font-bold text-center">Installation</h2>
        <div className="max-w-2xl mx-auto">
          <CodeBlock
            code="dotnet add package LowlandTech.TinyTools"
            language="bash"
          />
        </div>
      </section>
    </div>
  );
}

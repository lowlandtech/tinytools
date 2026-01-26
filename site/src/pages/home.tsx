import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { CodeBlock } from "@/components/code-block";
import { ArrowRight, Zap, Code2, Layers, Settings } from "lucide-react";

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
    description: "Optimized for code generation scenarios with minimal allocations.",
    href: "/getting-started",
  },
];

const quickExample = `// Create a template
var template = @"
@foreach(var item in Context.Items) {
    public ${item.Type} ${item.Name} { get; set; }
}
";

// Set up context
var context = new ExecutionContext();
context.Set("Items", new[] {
    new { Type = "string", Name = "FirstName" },
    new { Type = "int", Name = "Age" }
});

// Render
var engine = new TinyTemplateEngine();
var result = engine.Render(template, context);`;

export function HomePage() {
  return (
    <div className="space-y-16">
      {/* Hero Section */}
      <section className="text-center space-y-6 py-12">
        <div className="inline-flex items-center rounded-full border px-4 py-1.5 text-sm font-medium">
          <span className="text-secondary">??</span>
          <span className="ml-2 text-muted-foreground">
            Factory tools for .NET code generation
          </span>
        </div>
        <h1 className="text-4xl font-bold tracking-tight sm:text-6xl">
          <span className="text-primary">Factory</span>Tools
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
        <h2 className="text-2xl font-bold text-center">Quick Example</h2>
        <div className="max-w-4xl mx-auto">
          <CodeBlock code={quickExample} language="csharp" />
        </div>
      </section>

      {/* Installation */}
      <section className="space-y-6">
        <h2 className="text-2xl font-bold text-center">Installation</h2>
        <div className="max-w-2xl mx-auto">
          <CodeBlock
            code="dotnet add package LowlandTech.FactoryTools"
            language="bash"
          />
        </div>
      </section>
    </div>
  );
}

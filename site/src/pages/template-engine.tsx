import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

const ifExample = `@if(Context.Count > 0) {
    <p>Items found: \${Context.Count}</p>
} else if (Context.ShowEmpty) {
    <p>No items, but showing empty state</p>
} else {
    <p>No items found</p>
}`;

const foreachExample = `@foreach(var item in Context.Items) {
    public \${item.Type} \${item.Name} { get; set; }
}`;

const commentExample = `@* This is a comment that won't appear in output *@
public class \${Context.ClassName}
{
    @* Generate properties *@
    @foreach(var prop in Context.Properties) {
        public \${prop.Type} \${prop.Name} { get; set; }
    }
}`;

const comparisonExample = `@if(Context.Value >= 10) {
    Large value
}

@if(Context.Name == "Admin") {
    Admin user detected
}

@if(Context.IsEnabled != false) {
    Feature is enabled
}`;

export function TemplateEnginePage() {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <div className="space-y-4">
        <h1 className="text-3xl font-bold">Template Engine</h1>
        <p className="text-lg text-muted-foreground">
          The TinyTemplateEngine provides Razor-like syntax for control flow with
          <code className="mx-1 px-1 py-0.5 bg-muted rounded">@if</code>,
          <code className="mx-1 px-1 py-0.5 bg-muted rounded">@foreach</code>,
          and comments.
        </p>
      </div>

      <Tabs defaultValue="if" className="space-y-4">
        <TabsList>
          <TabsTrigger value="if">@if / @else</TabsTrigger>
          <TabsTrigger value="foreach">@foreach</TabsTrigger>
          <TabsTrigger value="comments">Comments</TabsTrigger>
          <TabsTrigger value="operators">Operators</TabsTrigger>
        </TabsList>

        <TabsContent value="if" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Conditional Rendering</CardTitle>
              <CardDescription>
                Use @if, @else if, and @else for conditional content.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <CodeBlock code={ifExample} language="razor" />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="foreach" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Iteration</CardTitle>
              <CardDescription>
                Loop over collections with @foreach.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <CodeBlock code={foreachExample} language="razor" />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="comments" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Template Comments</CardTitle>
              <CardDescription>
                Comments using @* ... *@ are removed from output.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <CodeBlock code={commentExample} language="razor" />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="operators" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Comparison Operators</CardTitle>
              <CardDescription>
                Supported operators: ==, !=, &gt;, &lt;, &gt;=, &lt;=, ! (negation)
              </CardDescription>
            </CardHeader>
            <CardContent>
              <CodeBlock code={comparisonExample} language="razor" />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <Card>
        <CardHeader>
          <CardTitle>Usage Example</CardTitle>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`var engine = new TinyTemplateEngine();
var context = new ExecutionContext();
context.Set("Count", 5);
context.Set("Items", new[] { 
    new { Type = "string", Name = "Id" },
    new { Type = "DateTime", Name = "CreatedAt" }
});

var result = engine.Render(template, context);`}
            language="csharp"
          />
        </CardContent>
      </Card>
    </div>
  );
}

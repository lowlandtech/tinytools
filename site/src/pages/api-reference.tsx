import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { CodeBlock } from "@/components/code-block";

export function ApiReferencePage() {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <div className="space-y-4">
        <h1 className="text-3xl font-bold">API Reference</h1>
        <p className="text-lg text-muted-foreground">
          Complete API documentation for FactoryTools.
        </p>
      </div>

      {/* TinyTemplateEngine */}
      <Card>
        <CardHeader>
          <CardTitle>TinyTemplateEngine</CardTitle>
          <CardDescription>
            The main template engine class for rendering templates.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h4 className="font-semibold mb-2">Methods</h4>
            <div className="space-y-4">
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="string Render(string template, ExecutionContext context)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2">
                  Renders a template with the given execution context. Processes control flow
                  directives (@if, @foreach) and resolves variables.
                </p>
              </div>
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="string ResolveVariables(string input, ExecutionContext context)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2">
                  Resolves only variable interpolation without processing control flow.
                </p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* ExecutionContext */}
      <Card>
        <CardHeader>
          <CardTitle>ExecutionContext</CardTitle>
          <CardDescription>
            Hierarchical context for storing template data.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h4 className="font-semibold mb-2">Methods</h4>
            <div className="space-y-4">
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="void Set(string key, object? value)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2">
                  Sets a value in the context with the specified key.
                </p>
              </div>
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="T? Get<T>(string key)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2">
                  Gets a value from the context, checking parent contexts if not found locally.
                </p>
              </div>
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="ExecutionContext CreateChild()"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2">
                  Creates a child context that inherits from this context.
                </p>
              </div>
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="bool TryGet<T>(string key, out T? value)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2">
                  Tries to get a value, returning false if not found.
                </p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Template Syntax */}
      <Card>
        <CardHeader>
          <CardTitle>Template Syntax Reference</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-2 font-semibold">Syntax</th>
                  <th className="text-left py-2 font-semibold">Description</th>
                </tr>
              </thead>
              <tbody className="text-muted-foreground">
                <tr className="border-b">
                  <td className="py-2"><code>${"{Context.Name}"}</code></td>
                  <td className="py-2">Variable interpolation</td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>@if(condition) {"{ }"}</code></td>
                  <td className="py-2">Conditional block</td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>{"}"} else {"{"}</code></td>
                  <td className="py-2">Else block</td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>{"}"} else if(condition) {"{"}</code></td>
                  <td className="py-2">Else-if block</td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>@foreach(var x in Context.Items) {"{ }"}</code></td>
                  <td className="py-2">Loop block</td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>@* comment *@</code></td>
                  <td className="py-2">Template comment (removed from output)</td>
                </tr>
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {/* Operators */}
      <Card>
        <CardHeader>
          <CardTitle>Condition Operators</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-2 font-semibold">Operator</th>
                  <th className="text-left py-2 font-semibold">Example</th>
                </tr>
              </thead>
              <tbody className="text-muted-foreground">
                <tr className="border-b">
                  <td className="py-2"><code>==</code></td>
                  <td className="py-2"><code>Context.Name == "Admin"</code></td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>!=</code></td>
                  <td className="py-2"><code>Context.Status != "Deleted"</code></td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>&gt;</code></td>
                  <td className="py-2"><code>Context.Count &gt; 0</code></td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>&lt;</code></td>
                  <td className="py-2"><code>Context.Age &lt; 18</code></td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>&gt;=</code></td>
                  <td className="py-2"><code>Context.Score &gt;= 50</code></td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>&lt;=</code></td>
                  <td className="py-2"><code>Context.Level &lt;= 10</code></td>
                </tr>
                <tr className="border-b">
                  <td className="py-2"><code>!</code></td>
                  <td className="py-2"><code>!Context.IsDisabled</code></td>
                </tr>
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

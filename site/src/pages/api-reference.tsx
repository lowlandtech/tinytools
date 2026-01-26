import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { CodeBlock } from "@/components/code-block";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

export function ApiReferencePage() {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <div className="space-y-4">
        <h1 className="text-3xl font-bold">API Reference</h1>
        <p className="text-lg text-muted-foreground">
          Complete API documentation for FactoryTools.
        </p>
      </div>

      {/* String Interpolation Extension */}
      <Card>
        <CardHeader>
          <CardTitle>String Interpolation Extensions</CardTitle>
          <CardDescription>
            Simple tag-based string replacement - the base case for quick templates.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="border-l-2 border-primary pl-4">
            <CodeBlock
              code="string Interpolate(this string template, object model)"
              language="csharp"
            />
            <p className="text-sm text-muted-foreground mt-2 mb-2">
              Replaces {"{PropertyName}"} tags with values from an anonymous object or class.
            </p>
            <CodeBlock
              code={`var template = "Hello {FirstName} {LastName}!";
var model = new { FirstName = "John", LastName = "Smith" };
var result = template.Interpolate(model);
// Output: "Hello John Smith!"`}
              language="csharp"
            />
          </div>
          
          <div className="border-l-2 border-primary pl-4">
            <CodeBlock
              code="string Interpolate(this string template, Dictionary<string, string> data)"
              language="csharp"
            />
            <p className="text-sm text-muted-foreground mt-2 mb-2">
              Replaces {"{Key}"} tags with values from a dictionary.
            </p>
            <CodeBlock
              code={`var template = "Welcome to {City}, {Country}!";
var data = new Dictionary<string, string>
{
    { "City", "Amsterdam" },
    { "Country", "Netherlands" }
};
var result = template.Interpolate(data);
// Output: "Welcome to Amsterdam, Netherlands!"`}
              language="csharp"
            />
          </div>
        </CardContent>
      </Card>

      {/* TinyTemplateEngine */}
      <Card>
        <CardHeader>
          <CardTitle>TinyTemplateEngine</CardTitle>
          <CardDescription>
            The main template engine class for advanced templating with control flow.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h4 className="font-semibold mb-2">Constructor</h4>
            <CodeBlock
              code="var engine = new TinyTemplateEngine();"
              language="csharp"
            />
          </div>
          
          <div>
            <h4 className="font-semibold mb-2">Methods</h4>
            <div className="space-y-4">
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="string Render(string template, ExecutionContext context)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2 mb-2">
                  Renders a template with full support for control flow (@if, @foreach) and variable interpolation.
                </p>
                <CodeBlock
                  code={`var engine = new TinyTemplateEngine();
var context = new ExecutionContext();
context.Set("Name", "Alice");
context.Set("IsPremium", true);

var template = """
    Hello \${Context.Name}!
    @if (Context.IsPremium) {
    ? Premium Member
    }
    """;

var result = engine.Render(template, context);`}
                  language="csharp"
                />
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
            Hierarchical context for storing template data with parent-child relationships.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h4 className="font-semibold mb-2">Properties</h4>
            <div className="space-y-2">
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock code="object? Model { get; set; }" language="csharp" />
                <p className="text-sm text-muted-foreground mt-1 mb-2">
                  Primary model object accessible via ${"${Context.Model.Property}"}
                </p>
                <CodeBlock
                  code={`var context = new ExecutionContext
{
    Model = new Customer { Name = "John", Email = "john@example.com" }
};

var template = "Email: \${Context.Model.Email}";`}
                  language="csharp"
                />
              </div>
            </div>
          </div>
          
          <div>
            <h4 className="font-semibold mb-2">Methods</h4>
            <div className="space-y-4">
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="void Set(string key, object? value)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2 mb-2">
                  Sets a value in the context with the specified key.
                </p>
                <CodeBlock
                  code={`context.Set("Name", "John");
context.Set("Age", 30);
context.Set("Items", new[] { "A", "B", "C" });`}
                  language="csharp"
                />
              </div>
              
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="T? Get<T>(string key)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2 mb-2">
                  Gets a value from the context, checking parent contexts if not found locally.
                </p>
                <CodeBlock
                  code={`var name = context.Get<string>("Name");
var age = context.Get<int>("Age");`}
                  language="csharp"
                />
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
              
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="ExecutionContext CreateChild()"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2">
                  Creates a child context that inherits from this context. Used internally for @foreach loops.
                </p>
              </div>
              
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="void RegisterService(string name, TemplateServiceFunc func)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2 mb-2">
                  Registers a template service function with a name.
                </p>
                <CodeBlock
                  code={`context.RegisterService("upper", 
    input => input?.ToString()?.ToUpper());

// Use in template
var template = "\${Context.Services('upper')('hello')}";
// Output: "HELLO"`}
                  language="csharp"
                />
              </div>
              
              <div className="border-l-2 border-primary pl-4">
                <CodeBlock
                  code="void RegisterService(ITemplateService service)"
                  language="csharp"
                />
                <p className="text-sm text-muted-foreground mt-2 mb-2">
                  Registers a template service instance (for IoC/DI integration).
                </p>
                <CodeBlock
                  code={`public class MyService : ITemplateService
{
    public string Name => "pluralize";
    public object? Transform(object? input) => /* implementation */;
}

context.RegisterService(new MyService());`}
                  language="csharp"
                />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Template Syntax */}
      <Card>
        <CardHeader>
          <CardTitle>Template Syntax Reference</CardTitle>
          <CardDescription>
            Complete syntax guide for template features.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-6">
            <div>
              <h4 className="font-semibold mb-3">Variable Interpolation</h4>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left py-2 font-semibold">Syntax</th>
                      <th className="text-left py-2 font-semibold">Example</th>
                    </tr>
                  </thead>
                  <tbody className="text-muted-foreground">
                    <tr className="border-b">
                      <td className="py-2"><code>${"{Context.xxx}"}</code></td>
                      <td className="py-2"><code>${"{Context.Name}"}</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>${"{Context.Model.xxx}"}</code></td>
                      <td className="py-2"><code>${"{Context.Model.FirstName}"}</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>${"{expr ?? \"default\"}"}</code></td>
                      <td className="py-2"><code>${"{Context.Title ?? \"Untitled\"}"}</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>${"{expr | helper}"}</code></td>
                      <td className="py-2"><code>${"{Context.Name | upper}"}</code></td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <div>
              <h4 className="font-semibold mb-3">Control Flow</h4>
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
                      <td className="py-2"><code>@if (condition) {"{ }"}</code></td>
                      <td className="py-2">Conditional block</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>{"}"} else {"{"}</code></td>
                      <td className="py-2">Else block</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>{"}"} else if (condition) {"{"}</code></td>
                      <td className="py-2">Else-if chain</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>@foreach (var x in collection) {"{ }"}</code></td>
                      <td className="py-2">Loop over collection</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>@* comment *@</code></td>
                      <td className="py-2">Template comment (removed)</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <div>
              <h4 className="font-semibold mb-3">Comparison Operators</h4>
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
                      <td className="py-2"><code>@if (Context.Role == "admin")</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>!=</code></td>
                      <td className="py-2"><code>@if (Context.Status != "inactive")</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>&gt;</code></td>
                      <td className="py-2"><code>@if (Context.Age &gt; 18)</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>&gt;=</code></td>
                      <td className="py-2"><code>@if (Context.Score &gt;= 90)</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>&lt;</code></td>
                      <td className="py-2"><code>@if (Context.Count &lt; 10)</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>&lt;=</code></td>
                      <td className="py-2"><code>@if (Context.Price &lt;= 100)</code></td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>!</code></td>
                      <td className="py-2"><code>@if (!Context.IsExpired)</code></td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Pipe Helpers Reference */}
      <Card>
        <CardHeader>
          <CardTitle>Built-in Pipe Helpers</CardTitle>
          <CardDescription>
            Transform values using ${"${value | helper}"} or ${"${value | helper:arg}"} syntax.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="string">
            <TabsList className="grid w-full grid-cols-4">
              <TabsTrigger value="string">String</TabsTrigger>
              <TabsTrigger value="date">Date</TabsTrigger>
              <TabsTrigger value="number">Number</TabsTrigger>
              <TabsTrigger value="collection">Collection</TabsTrigger>
            </TabsList>
            
            <TabsContent value="string">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left py-2">Helper</th>
                      <th className="text-left py-2">Example</th>
                      <th className="text-left py-2">Output</th>
                    </tr>
                  </thead>
                  <tbody className="text-muted-foreground">
                    <tr className="border-b">
                      <td className="py-2"><code>upper</code></td>
                      <td className="py-2"><code>${"{Name | upper}"}</code></td>
                      <td className="py-2">JOHN</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>lower</code></td>
                      <td className="py-2"><code>${"{Name | lower}"}</code></td>
                      <td className="py-2">john</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>capitalize</code></td>
                      <td className="py-2"><code>${"{Name | capitalize}"}</code></td>
                      <td className="py-2">John</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>camelcase</code></td>
                      <td className="py-2"><code>${"{Name | camelcase}"}</code></td>
                      <td className="py-2">firstName</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>pascalcase</code></td>
                      <td className="py-2"><code>${"{Name | pascalcase}"}</code></td>
                      <td className="py-2">FirstName</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>trim</code></td>
                      <td className="py-2"><code>${"{Text | trim}"}</code></td>
                      <td className="py-2">hello</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>truncate:N</code></td>
                      <td className="py-2"><code>${"{Desc | truncate:20}"}</code></td>
                      <td className="py-2">This is a long te...</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>replace:old,new</code></td>
                      <td className="py-2"><code>${"{Path | replace:old,new}"}</code></td>
                      <td className="py-2">Replaces text</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>padleft:N,char</code></td>
                      <td className="py-2"><code>${"{Id | padleft:5,0}"}</code></td>
                      <td className="py-2">00042</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>padright:N,char</code></td>
                      <td className="py-2"><code>${"{Name | padright:10,.}"}</code></td>
                      <td className="py-2">John......</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </TabsContent>
            
            <TabsContent value="date">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left py-2">Helper</th>
                      <th className="text-left py-2">Example</th>
                      <th className="text-left py-2">Output</th>
                    </tr>
                  </thead>
                  <tbody className="text-muted-foreground">
                    <tr className="border-b">
                      <td className="py-2"><code>format:pattern</code></td>
                      <td className="py-2"><code>${"{Date | format:yyyy-MM-dd}"}</code></td>
                      <td className="py-2">2024-06-15</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>date</code></td>
                      <td className="py-2"><code>${"{Date | date}"}</code></td>
                      <td className="py-2">2024-06-15</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>date:pattern</code></td>
                      <td className="py-2"><code>${"{Date | date:dd-MMM-yyyy}"}</code></td>
                      <td className="py-2">15-Jun-2024</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </TabsContent>
            
            <TabsContent value="number">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left py-2">Helper</th>
                      <th className="text-left py-2">Example</th>
                      <th className="text-left py-2">Output</th>
                    </tr>
                  </thead>
                  <tbody className="text-muted-foreground">
                    <tr className="border-b">
                      <td className="py-2"><code>number</code></td>
                      <td className="py-2"><code>${"{Value | number}"}</code></td>
                      <td className="py-2">1,234</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>format:N2</code></td>
                      <td className="py-2"><code>${"{Price | format:N2}"}</code></td>
                      <td className="py-2">1,234.57</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>format:C</code></td>
                      <td className="py-2"><code>${"{Price | format:C}"}</code></td>
                      <td className="py-2">$1,234.57</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>format:P0</code></td>
                      <td className="py-2"><code>${"{Rate | format:P0}"}</code></td>
                      <td className="py-2">86%</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>round:N</code></td>
                      <td className="py-2"><code>${"{Pi | round:2}"}</code></td>
                      <td className="py-2">3.14</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>floor</code></td>
                      <td className="py-2"><code>${"{Value | floor}"}</code></td>
                      <td className="py-2">3</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>ceiling</code></td>
                      <td className="py-2"><code>${"{Value | ceiling}"}</code></td>
                      <td className="py-2">4</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </TabsContent>
            
            <TabsContent value="collection">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left py-2">Helper</th>
                      <th className="text-left py-2">Example</th>
                      <th className="text-left py-2">Output</th>
                    </tr>
                  </thead>
                  <tbody className="text-muted-foreground">
                    <tr className="border-b">
                      <td className="py-2"><code>count</code></td>
                      <td className="py-2"><code>${"{Items | count}"}</code></td>
                      <td className="py-2">5</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>first</code></td>
                      <td className="py-2"><code>${"{Items | first}"}</code></td>
                      <td className="py-2">First item</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>last</code></td>
                      <td className="py-2"><code>${"{Items | last}"}</code></td>
                      <td className="py-2">Last item</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>join:sep</code></td>
                      <td className="py-2"><code>${"{Tags | join:, }"}</code></td>
                      <td className="py-2">a, b, c</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>reverse</code></td>
                      <td className="py-2"><code>${"{Word | reverse}"}</code></td>
                      <td className="py-2">olleH</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>default:value</code></td>
                      <td className="py-2"><code>${"{Name | default:Guest}"}</code></td>
                      <td className="py-2">Guest (if null)</td>
                    </tr>
                    <tr className="border-b">
                      <td className="py-2"><code>yesno</code></td>
                      <td className="py-2"><code>${"{Active | yesno}"}</code></td>
                      <td className="py-2">Yes / No</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </TabsContent>
          </Tabs>
          
          <div className="mt-4 p-4 bg-muted rounded-lg">
            <h4 className="font-semibold mb-2">Chaining Helpers</h4>
            <CodeBlock
              code={`\${Context.Name | trim | upper | truncate:20}
\${Context.Items | first | upper}
\${Context.Date | format:MMMM | upper}`}
              language="csharp"
            />
          </div>
        </CardContent>
      </Card>

      {/* ITemplateService */}
      <Card>
        <CardHeader>
          <CardTitle>ITemplateService Interface</CardTitle>
          <CardDescription>
            Create custom template services for extensibility.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="border-l-2 border-primary pl-4">
            <CodeBlock
              code={`public interface ITemplateService
{
    string Name { get; }
    object? Transform(object? input);
}

public delegate object? TemplateServiceFunc(object? input);`}
              language="csharp"
            />
          </div>
          
          <div>
            <h4 className="font-semibold mb-2">Example Implementation</h4>
            <CodeBlock
              code={`public class PluralizationService : ITemplateService
{
    public string Name => "pluralize";
    
    public object? Transform(object? input)
    {
        return input?.ToString()?.Pluralize(); // Using Humanizer
    }
}

// Register
context.RegisterService(new PluralizationService());

// Use in template
var template = "\${Context.Services('pluralize')('customer')}";
// Output: "customers"`}
              language="csharp"
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

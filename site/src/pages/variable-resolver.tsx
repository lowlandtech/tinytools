import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

export function VariableResolverPage() {
return (
  <div className="max-w-4xl mx-auto space-y-8">
    <div className="space-y-4">
      <p className="text-lg text-muted-foreground">
        The VariableResolver supports two interpolation styles: simple <code className="px-1 py-0.5 bg-muted rounded">{"...}"}</code> tag-based
        and engine-backed <code className="px-1 py-0.5 bg-muted rounded">${"${...}"}</code> interpolation with full template features.
      </p>
    </div>

      <Card>
        <CardHeader>
          <CardTitle>Interpolation Styles</CardTitle>
          <CardDescription>
            Choose the right interpolation style for your use case.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="simple">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="simple">Simple {"{PropertyName}"}</TabsTrigger>
              <TabsTrigger value="engine">Engine ${"${Context.xxx}"}</TabsTrigger>
            </TabsList>
            <TabsContent value="simple" className="space-y-4">
              <p className="text-sm text-muted-foreground">
                Use the <code>Interpolate</code> extension for quick, tag-based replacements.
              </p>
              <CodeBlock
                code={`var template = "Hello {FirstName} {LastName}";
var model = new { FirstName = "John", LastName = "Smith" };

var result = template.Interpolate(model);
// Output: Hello John Smith`}
                language="csharp"
              />
              <p className="text-sm text-muted-foreground mt-4">
                Dictionary models are also supported:
              </p>
              <CodeBlock
                code={`var template = "Hello {Name}";
var model = new Dictionary<string, string> { ["Name"] = "Jane" };
var result = template.Interpolate(model);
// Output: Hello Jane`}
                language="csharp"
              />
            </TabsContent>
            <TabsContent value="engine" className="space-y-4">
              <p className="text-sm text-muted-foreground">
                Use ExecutionContext for ${"${Context.xxx}"} expressions and all advanced features.
              </p>
              <CodeBlock
                code={`var context = new ExecutionContext();
context.Set("Name", "John");
context.Set("Age", 30);

var template = "Hello, \${Context.Name}! You are \${Context.Age} years old.";
var result = engine.Render(template, context);
// Output: Hello, John! You are 30 years old.`}
                language="csharp"
              />
            </TabsContent>
          </Tabs>
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

      <Card>
        <CardHeader>
          <CardTitle>Pipe Helpers</CardTitle>
          <CardDescription>
            Transform values using the pipe syntax: ${"${Context.Value | helper}"} or ${"${Context.Value | helper:argument}"}
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
            <TabsContent value="string" className="space-y-4">
              <CodeBlock
                code={`// String helpers
Upper: \${Context.Name | upper}           // JOHN
Lower: \${Context.Name | lower}           // john
Capitalize: \${Context.Name | capitalize} // John
CamelCase: \${Context.Name | camelcase}  // firstName
PascalCase: \${Context.Name | pascalcase} // FirstName
Trim: \${Context.Text | trim}             // "hello"
Truncate: \${Context.Desc | truncate:20}  // "This is a long te..."
Replace: \${Context.Path | replace:old,new}
PadLeft: \${Context.Id | padleft:5,0}     // 00042
PadRight: \${Context.Name | padright:10,.} // John......`}
                language="csharp"
              />
            </TabsContent>
            <TabsContent value="date" className="space-y-4">
              <CodeBlock
                code={`// Date helpers
ISO Date: \${Context.OrderDate | format:yyyy-MM-dd}     // 2024-06-15
US Date: \${Context.OrderDate | format:MM/dd/yyyy}      // 06/15/2024
Time: \${Context.OrderDate | format:HH:mm}              // 14:30
Default Date: \${Context.BirthDate | date}              // 2024-06-15
Custom Date: \${Context.BirthDate | date:dd-MMM-yyyy}  // 15-Jun-2024`}
                language="csharp"
              />
            </TabsContent>
            <TabsContent value="number" className="space-y-4">
              <CodeBlock
                code={`// Number helpers
Formatted: \${Context.Quantity | number}      // 1,234
Decimals: \${Context.Price | format:N2}       // 1,234.57
Currency: \${Context.Price | format:C}        // $1,234.57
Percent: \${Context.Rate | format:P0}         // 86%
Round: \${Context.Pi | round:2}               // 3.14
Floor: \${Context.Value | floor}              // 3
Ceiling: \${Context.Value | ceiling}          // 4`}
                language="csharp"
              />
            </TabsContent>
            <TabsContent value="collection" className="space-y-4">
              <CodeBlock
                code={`// Collection helpers
Count: \${Context.Items | count}              // 5
First: \${Context.Items | first}              // First item
Last: \${Context.Items | last}                // Last item
Join: \${Context.Tags | join:, }              // "a, b, c"
Reverse: \${Context.Word | reverse}           // "olleH"`}
                language="csharp"
              />
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Chaining Helpers</CardTitle>
          <CardDescription>
            Pipe multiple helpers together to create powerful transformations.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CodeBlock
            code={`// Chain multiple helpers
\${Context.Name | trim | upper | truncate:20}
\${Context.Items | first | upper}
\${Context.Date | format:MMMM | upper}  // "JANUARY"`}
            language="csharp"
          />
        </CardContent>
      </Card>
    </div>
  );
}

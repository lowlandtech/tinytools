import { CodeBlock } from "@/components/code-block";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function CodeGenerationPage() {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <div className="space-y-4">
        <h1 className="text-3xl font-bold">Code Generation</h1>
        <p className="text-lg text-muted-foreground">
          Use TinyTools to generate C# classes, DTOs, APIs, and other code artifacts dynamically.
          Perfect for scaffolding tools and source generators.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Basic Class Generation</CardTitle>
          <CardDescription>
            Generate simple C# classes with properties.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var engine = new TinyTemplateEngine();
var context = new ExecutionContext();
context.Set("ClassName", "Customer");
context.Set("Properties", new[]
{
    new { Name = "Id", Type = "int" },
    new { Name = "Name", Type = "string" },
    new { Name = "Email", Type = "string" },
    new { Name = "IsActive", Type = "bool" }
});

var template = """
    public class \${Context.ClassName}
    {
        @foreach(var prop in Context.Properties) {
        public \${prop.Type} \${prop.Name} { get; set; }
        }
    }
    """;

var code = engine.Render(template, context);`}
            language="csharp"
          />
          <div className="mt-4">
            <p className="text-sm font-medium mb-2">Output:</p>
            <CodeBlock
              code={`public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}`}
              language="csharp"
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>DTOs with Validation</CardTitle>
          <CardDescription>
            Generate data transfer objects with data annotations.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var context = new ExecutionContext();
context.Set("ClassName", "CreateUserDto");
context.Set("Properties", new[]
{
    new { Name = "Username", Type = "string", Required = true, MaxLength = 50 },
    new { Name = "Email", Type = "string", Required = true, MaxLength = 100 },
    new { Name = "Age", Type = "int?", Required = false, MaxLength = (int?)null }
});

var template = """
    public class \${Context.ClassName}
    {
        @foreach(var prop in Context.Properties) {
        @if (prop.Required) {
        [Required]
        }
        @if (prop.MaxLength != null) {
        [MaxLength(\${prop.MaxLength})]
        }
        public \${prop.Type} \${prop.Name} { get; set; }

        }
    }
    """;

var code = engine.Render(template, context);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>API Controller Generation</CardTitle>
          <CardDescription>
            Scaffold complete API controllers with CRUD operations.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var context = new ExecutionContext();
context.Set("EntityName", "Product");
context.Set("EntityNamePlural", "Products");
context.Set("EntityVariable", "product");

var template = """
    [ApiController]
    [Route("api/[controller]")]
    public class \${Context.EntityNamePlural}Controller : ControllerBase
    {
        private readonly IRepository<\${Context.EntityName}> _repository;

        public \${Context.EntityNamePlural}Controller(IRepository<\${Context.EntityName}> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<\${Context.EntityName}>>> GetAll()
        {
            var items = await _repository.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<\${Context.EntityName}>> GetById(int id)
        {
            var \${Context.EntityVariable} = await _repository.GetByIdAsync(id);
            if (\${Context.EntityVariable} == null)
                return NotFound();
            
            return Ok(\${Context.EntityVariable});
        }

        [HttpPost]
        public async Task<ActionResult<\${Context.EntityName}>> Create(\${Context.EntityName} \${Context.EntityVariable})
        {
            await _repository.AddAsync(\${Context.EntityVariable});
            return CreatedAtAction(nameof(GetById), new { id = \${Context.EntityVariable}.Id }, \${Context.EntityVariable});
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, \${Context.EntityName} \${Context.EntityVariable})
        {
            if (id != \${Context.EntityVariable}.Id)
                return BadRequest();

            await _repository.UpdateAsync(\${Context.EntityVariable});
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
    """;

var code = engine.Render(template, context);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Entity Framework DbContext</CardTitle>
          <CardDescription>
            Generate DbContext configurations for multiple entities.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var context = new ExecutionContext();
context.Set("ContextName", "ApplicationDbContext");
context.Set("Entities", new[]
{
    new { Name = "User", TableName = "Users" },
    new { Name = "Product", TableName = "Products" },
    new { Name = "Order", TableName = "Orders" }
});

var template = """
    public class \${Context.ContextName} : DbContext
    {
        public \${Context.ContextName}(DbContextOptions<\${Context.ContextName}> options)
            : base(options)
        {
        }

        @foreach(var entity in Context.Entities) {
        public DbSet<\${entity.Name}> \${entity.Name}s { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            @foreach(var entity in Context.Entities) {
            modelBuilder.Entity<\${entity.Name}>().ToTable("\${entity.TableName}");
            }
        }
    }
    """;

var code = engine.Render(template, context);`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Source Generator Integration</CardTitle>
          <CardDescription>
            Use TinyTools in C# source generators for compile-time code generation.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`[Generator]
public class DtoGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var engine = new TinyTemplateEngine();
        var execContext = new ExecutionContext();
        
        execContext.Set("Namespace", "MyApp.Generated");
        execContext.Set("ClassName", "AutoGeneratedDto");
        execContext.Set("Properties", new[]
        {
            new { Name = "Id", Type = "int" },
            new { Name = "Name", Type = "string" }
        });

        var template = """
            namespace \${Context.Namespace};

            public class \${Context.ClassName}
            {
                @foreach(var prop in Context.Properties) {
                public \${prop.Type} \${prop.Name} { get; set; }
                }
            }
            """;

        var code = engine.Render(template, execContext);
        
        context.AddSource("\${execContext.Get("ClassName")}.g.cs", code);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Initialization logic
    }
}`}
            language="csharp"
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Multiple File Generation</CardTitle>
          <CardDescription>
            Generate multiple related files from a single data model.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CodeBlock
            code={`var engine = new TinyTemplateEngine();
var context = new ExecutionContext();
context.Set("EntityName", "Order");

// Generate Entity
var entityTemplate = """
    public class \${Context.EntityName}
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal Total { get; set; }
    }
    """;

// Generate Repository Interface
var repoTemplate = """
    public interface I\${Context.EntityName}Repository
    {
        Task<\${Context.EntityName}> GetByIdAsync(int id);
        Task<IEnumerable<\${Context.EntityName}>> GetAllAsync();
        Task AddAsync(\${Context.EntityName} entity);
        Task UpdateAsync(\${Context.EntityName} entity);
        Task DeleteAsync(int id);
    }
    """;

// Generate Service
var serviceTemplate = """
    public class \${Context.EntityName}Service
    {
        private readonly I\${Context.EntityName}Repository _repository;

        public \${Context.EntityName}Service(I\${Context.EntityName}Repository repository)
        {
            _repository = repository;
        }

        public async Task<\${Context.EntityName}> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
    """;

var entity = engine.Render(entityTemplate, context);
var repository = engine.Render(repoTemplate, context);
var service = engine.Render(serviceTemplate, context);

// Write to files
File.WriteAllText("Order.cs", entity);
File.WriteAllText("IOrderRepository.cs", repository);
File.WriteAllText("OrderService.cs", service);`}
            language="csharp"
          />
        </CardContent>
      </Card>
    </div>
  );
}

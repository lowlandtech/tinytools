using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "09")]
[UserStory("02", "Template engine renders a report that combines every feature")]
public sealed class SC09_RendersReportAndHelpers : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private const string Report = """
        ${Context.Title}
        =================

        @* Loop through all customers *@
        @foreach(var customer in Context.Customers) {
        Customer: ${customer.Name}
        Age: ${customer.Age}
        @if(customer.IsActive) {
        Status: ACTIVE
        } else {
        Status: INACTIVE
        }
        @if(customer.Age >= 30) {
        Category: Senior
        } else {
        Category: Junior
        }
        ---
        }

        End of Report
        """;

    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC09_RendersReportAndHelpers(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context holding customers, a report title, a name and products")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Customers", new[]
        {
            new { Name = "John", Age = 30, IsActive = true },
            new { Name = "Jane", Age = 25, IsActive = false },
            new { Name = "Bob", Age = 35, IsActive = true }
        });
        _context.Set("Title", "Customer Report");
        _context.Set("Name", "john doe");
        _context.Set("Products", new[]
        {
            new { Name = "Product A", Price = 10, InStock = true },
            new { Name = "Product B", Price = 20, InStock = false },
            new { Name = "Product C", Price = 15, InStock = true }
        });
    }

    [When("the report, helper and product templates are rendered once")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Report"] = Sut.Render(Report, _context);
        useCase.Cases["Uppercased"] = Sut.Render("Hello, ${Context.Name | upper}!", _context);
        useCase.Cases["Products"] = Sut.Render("Available Products:\n@foreach(var product in Context.Products) {\n@if(product.InStock) {\n- ${product.Name}: $${product.Price}\n}\n}", _context);
    }

    [Then("the customer report should contain every rendered section")]
    [Trait("UAC", "47"), Fact]
    public void UAC47_HandlesComplexTemplateWithAllFeatures() => ((string)useCase.Cases["Report"]!).Should().Contain("Customer Report").And.Contain("Customer: John").And.Contain("Customer: Jane").And.Contain("Customer: Bob").And.Contain("Status: ACTIVE").And.Contain("Status: INACTIVE").And.Contain("Category: Senior").And.Contain("Category: Junior").And.Contain("End of Report");

    [Then("the upper helper output should be exact")]
    [Trait("UAC", "48"), Fact]
    public void UAC48_HandlesTemplateWithVariablesAndHelpers() => ((string)useCase.Cases["Uppercased"]!).Should().Be("Hello, JOHN DOE!");

    [Then("the in-stock products should contain the available lines")]
    [Trait("UAC", "49"), Fact]
    public void UAC49_HandlesForeachWithConditionalRendering() => ((string)useCase.Cases["Products"]!).Should().Contain("Product A: $10").And.Contain("Product C: $15").And.NotContain("Product B");
}

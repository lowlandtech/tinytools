using ExecutionContext = LowlandTech.TinyTools.Core.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "06")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingOrderConfirmationEmailTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        var customer = new Customer
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@example.com",
            Orders =
            [
                new Order
                {
                    OrderNumber = "ORD-2024-001",
                    OrderDate = new DateTime(2024, 6, 15),
                    Total = "149.97",
                    Items =
                    [
                        new OrderItem { ProductName = "Wireless Mouse", Quantity = 2, Price = "29.99" },
                        new OrderItem { ProductName = "USB-C Hub", Quantity = 1, Price = "89.99" }
                    ]
                }
            ]
        };

        _context = new ExecutionContext { Model = customer };

        _template = """
            Hi ${Context.Model.FirstName},

            Thank you for your order!

            Order Details:
            @foreach (var order in Context.Model.Orders) {
            Order #${order.OrderNumber}
            
            Items:
            @foreach (var item in order.Items) {
            - ${item.ProductName} (Qty: ${item.Quantity}) - $${item.Price}
            }
            
            Total: $${order.Total}
            }

            We'll send you a shipping confirmation once your order is on its way.

            Thanks for shopping with us!
            """;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Customer Name")]
    [Fact]
    public void ItShouldRenderCustomerName()
    {
        ArrangeAndAct();
        _result.Should().Contain("Hi Jane,");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Order Number")]
    [Fact]
    public void ItShouldRenderOrderNumber()
    {
        ArrangeAndAct();
        _result.Should().Contain("Order #ORD-2024-001");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Render All Order Items")]
    [Fact]
    public void ItShouldRenderAllOrderItems()
    {
        ArrangeAndAct();
        _result.Should().Contain("Wireless Mouse (Qty: 2)");
        _result.Should().Contain("USB-C Hub (Qty: 1)");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Render Order Total")]
    [Fact]
    public void ItShouldRenderOrderTotal()
    {
        ArrangeAndAct();
        _result.Should().Contain("Total: $149.97");
    }
}

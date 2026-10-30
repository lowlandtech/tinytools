using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "06")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC06_GeneratesOrderConfirmation : UnitScenario
{
    private readonly TinyTemplateEngine _engine = new();
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = null!;

    [Given("an order confirmation template with one customer order")]
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

    [When("the engine renders the order confirmation template")]
    protected override void When() => _result = _engine.Render(_template, _context);

    public SC06_GeneratesOrderConfirmation() => ArrangeAndAct();

    [Then("the content should contain the customer name")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheCustomerName() => _result.Should().Contain("Hi Jane,");

    [Then("the content should contain the order number")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheOrderNumber() => _result.Should().Contain("Order #ORD-2024-001");

    [Then("the content should contain all order items")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersAllOrderItems() => _result.Should().Contain("Wireless Mouse (Qty: 2)").And.Contain("USB-C Hub (Qty: 1)");

    [Then("the content should contain the order total")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTheOrderTotal() => _result.Should().Contain("Total: $149.97");
}

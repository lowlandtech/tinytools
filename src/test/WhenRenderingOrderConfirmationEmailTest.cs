namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingOrderConfirmationEmailTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

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

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldRenderCustomerName()
    {
        _result.Should().Contain("Hi Jane,");
    }

    [Fact]
    public void ItShouldRenderOrderNumber()
    {
        _result.Should().Contain("Order #ORD-2024-001");
    }

    [Fact]
    public void ItShouldRenderAllOrderItems()
    {
        _result.Should().Contain("Wireless Mouse (Qty: 2)");
        _result.Should().Contain("USB-C Hub (Qty: 1)");
    }

    [Fact]
    public void ItShouldRenderOrderTotal()
    {
        _result.Should().Contain("Total: $149.97");
    }
}

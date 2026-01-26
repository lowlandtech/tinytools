namespace LowlandTech.TinyTools.UnitTests.Fakes;

internal class Customer
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Company { get; set; } = null!;
    public bool IsPremium { get; set; }
    public List<Order> Orders { get; set; } = [];
}

internal class Order
{
    public string OrderNumber { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public string Total { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = [];
}

internal class OrderItem
{
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public string Price { get; set; } = null!;
}

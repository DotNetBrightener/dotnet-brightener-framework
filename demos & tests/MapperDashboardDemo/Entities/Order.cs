namespace MapperDashboardDemo.Entities;

public class Order
{
    public int Id { get; set; }
    public User Customer { get; set; } = new();
    public List<OrderItem> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public Address? ShippingAddress { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

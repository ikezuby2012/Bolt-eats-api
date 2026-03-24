using SharedKernel;

namespace Domain.Order;

public sealed class OrderItem : Auditable<Guid>
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public Guid MenuItemId { get; set; }
    public MenuItem.MenuItem MenuItem { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

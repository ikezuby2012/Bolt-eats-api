using SharedKernel;

namespace Domain.Cart;

public sealed class CartItem : Auditable<Guid>
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; }
    public Guid MenuItemId { get; set; }
    public MenuItem.MenuItem MenuItem { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}

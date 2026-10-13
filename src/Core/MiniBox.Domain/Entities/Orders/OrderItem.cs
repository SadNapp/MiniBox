using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Orders;

public class OrderItem : BaseAuditableEntity
{
    public Guid OrderId { get; set; } 
    public Guid ProductId { get; set; } 
    
    public decimal UnitPrice { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

   public decimal TotalPrice => Quantity * UnitPrice;
   
}

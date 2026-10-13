using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Orders;

public class Order : BaseAuditableEntity
{
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeliveryStreet { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryPostalCode { get; set; } = string.Empty;
    public string DeliveryCountry { get; set; } = "Україна";
    public string? DeliveryNotes { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(item => item.TotalPrice);

}

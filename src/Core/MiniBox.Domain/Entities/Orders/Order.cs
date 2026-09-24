using System.Data;
using MiniBox.Domain.Common;
using MiniBox.Domain.Entities.Products;
using MiniBox.Domain.Entities.Users;

namespace MiniBox.Domain.Entities.Orders;

public class Order : BaseEntity
{
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Guid UserId { get; set; } 
    public User User { get; set; }
    public List<OrderItem> Items { get; set; }
}
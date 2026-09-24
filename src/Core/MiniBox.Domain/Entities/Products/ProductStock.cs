using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Products;

public class ProductStock : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
    
    public int TotalInStock => QuantityReserved + QuantityAvailable;
}
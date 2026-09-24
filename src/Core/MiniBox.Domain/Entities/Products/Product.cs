using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Products;

public class Product : BaseEntity
{
    public string ProductName { get; set; } =  string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public int ProductQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    
    
    
}
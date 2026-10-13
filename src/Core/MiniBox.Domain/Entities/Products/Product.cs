using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Products;

public class Product : BaseAuditableEntity
{
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public int ProductQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public ICollection<ProductAttribute> ProductAttributes { get; set; } = [];
}

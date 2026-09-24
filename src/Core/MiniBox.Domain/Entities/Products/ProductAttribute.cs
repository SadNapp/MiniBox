using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Products;

public class ProductAttribute : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public string Key { get; set; } = string.Empty; 
    public string Value { get; set; } = string.Empty;

}
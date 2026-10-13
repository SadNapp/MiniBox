
using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Products;

public class Category : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<Product> Products { get; set; } = [];
}

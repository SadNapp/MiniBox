
using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Products;

public class Category : BaseEntity
{
    public string NameCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<Product>? Products { get; set; } 
}
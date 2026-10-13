using MiniBox.Application.Common.Interfaces;
using MiniBox.Domain.Entities.Orders;
using MiniBox.Domain.Entities.Products;

namespace MiniBox.Application.Common.Mappings;

public static class MappingProfiles
{
    public static ProductDto ToProductDto(Product product, int stock) =>
        new(product.Id, product.Sku, product.ProductName, product.Description,
            product.Price * (1m - product.Discount / 100m), stock, product.Discount);

    public static OrderItemDto ToOrderItemDto(OrderItem item) =>
        new(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
}

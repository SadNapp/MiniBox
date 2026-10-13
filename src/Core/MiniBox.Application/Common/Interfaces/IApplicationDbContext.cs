using MiniBox.Domain.Entities.Orders;
using MiniBox.Domain.Entities.Products;
using MiniBox.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace MiniBox.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }
    DbSet<Category> Categories { get; }
    DbSet<ProductStock> ProductStocks { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record CreateOrderLine(Guid ProductId, int Quantity);
public sealed record CreateOrderCommand(string CustomerName, string Email, IReadOnlyList<CreateOrderLine> Items, Guid? UserId = null,
    string DeliveryStreet = "", string DeliveryCity = "", string DeliveryPostalCode = "", string DeliveryCountry = "Україна", string? DeliveryNotes = null);
public sealed record CreateOrderResult(Guid OrderId, string OrderNumber, decimal TotalPrice);
public sealed record OrderSummaryDto(Guid Id, string OrderNumber, DateTime CreatedAt, string Status, decimal TotalPrice, IReadOnlyList<OrderItemDto> Items,
    string DeliveryStreet, string DeliveryCity, string DeliveryPostalCode, string DeliveryCountry, string? DeliveryNotes);
public sealed record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
public sealed record ProductDto(Guid Id, string Sku, string Name, string Description, decimal Price, int Stock, decimal Discount = 0);
public sealed record ProductPage(IReadOnlyList<ProductDto> Items, int Page, int PageSize, int TotalCount);

public interface IOrderService
{
    Task<CreateOrderResult> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderSummaryDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IProductCatalog
{
    Task<ProductPage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken, string? search = null,
        string? attributeKey = null, string? attributeValue = null, string? category = null, decimal? minPrice = null,
        decimal? maxPrice = null, string? sortBy = null, string? sortDirection = null);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task InvalidateCatalogAsync(CancellationToken cancellationToken);
}

public interface IProductStockService
{
    Task<bool> SetAvailableAsync(Guid productId, int quantity, CancellationToken cancellationToken);
}

public sealed record AuthenticationResult(Guid UserId, string Email, string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, DateTime RefreshExpiresAtUtc, string FirstName = "", string LastName = "");

public interface IUserAuthenticationService
{
    Task<AuthenticationResult?> RegisterAsync(string firstName, string lastName, string email, string password, CancellationToken cancellationToken);
    Task<AuthenticationResult?> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthenticationResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
}

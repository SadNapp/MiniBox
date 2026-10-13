using Microsoft.EntityFrameworkCore;
using MiniBox.Domain.Entities.Orders;
using MiniBox.Domain.Entities.Products;
using MiniBox.Domain.Entities.Users;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Domain.Common;

namespace MiniBox.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService? currentUser = null,
    IDateTimeService? dateTime = null) : DbContext(options), IApplicationDbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(entity => typeof(BaseEntity).IsAssignableFrom(entity.ClrType)))
            modelBuilder.Entity(entityType.ClrType).Ignore(nameof(BaseEntity.DomainEvents));
        modelBuilder.Entity<ProductAttribute>(entity =>
        {
            entity.HasKey(attribute => attribute.Id);
            entity.Property(attribute => attribute.Key).HasMaxLength(100).IsRequired();
            entity.Property(attribute => attribute.Value).HasMaxLength(500).IsRequired();
            entity.HasIndex(attribute => new { attribute.ProductId, attribute.Key, attribute.Value });
            entity.HasOne(attribute => attribute.Product).WithMany(product => product.ProductAttributes).HasForeignKey(attribute => attribute.ProductId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.UnitPrice).HasPrecision(12, 2);
            entity.Property(item => item.ProductName).HasMaxLength(180).IsRequired();
            entity.Ignore(item => item.TotalPrice);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "\"Quantity\" > 0");
                table.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "\"UnitPrice\" >= 0");
            });
        });
    }

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = dateTime?.UtcNow ?? DateTime.UtcNow;
        var actor = currentUser?.UserId?.ToString();
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy = actor;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAtUtc).IsModified = false;
                entry.Property(entity => entity.CreatedBy).IsModified = false;
                entry.Entity.UpdatedAt = now;
                entry.Entity.LastModifiedAt = now;
                entry.Entity.LastModifiedBy = actor;
            }
        }
    }
}

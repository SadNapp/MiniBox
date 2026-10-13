using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBox.Domain.Entities.Products;

namespace MiniBox.Infrastructure.Persistence.Configurations;

public sealed class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
{
    public void Configure(EntityTypeBuilder<ProductStock> entity)
    {
        entity.HasKey(stock => stock.Id);
        entity.HasIndex(stock => stock.ProductId).IsUnique();
        entity.HasOne(stock => stock.Product).WithMany().HasForeignKey(stock => stock.ProductId).OnDelete(DeleteBehavior.Cascade);
        entity.Property(stock => stock.QuantityAvailable).IsConcurrencyToken();
        entity.Property<uint>("RowVersion").IsRowVersion();
        entity.ToTable(table => table.HasCheckConstraint("CK_ProductStocks_Quantities_NonNegative",
            "\"QuantityAvailable\" >= 0 AND \"QuantityReserved\" >= 0"));
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBox.Domain.Entities.Products;

namespace MiniBox.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.HasKey(product => product.Id);
        entity.Property(product => product.ProductName).HasMaxLength(180).IsRequired();
        entity.Property(product => product.Sku).HasMaxLength(64).IsRequired();
        entity.HasIndex(product => product.Sku).IsUnique();
        entity.Property(product => product.Description).HasMaxLength(2000);
        entity.Property(product => product.Price).HasPrecision(12, 2);
        entity.Property(product => product.Discount).HasPrecision(5, 2);
        entity.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Products_Price_NonNegative", "\"Price\" >= 0");
            table.HasCheckConstraint("CK_Products_Discount_Range", "\"Discount\" >= 0 AND \"Discount\" <= 100");
        });
        entity.HasIndex(product => product.ProductName);
        entity.HasOne(product => product.Category).WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId).OnDelete(DeleteBehavior.SetNull);
    }
}

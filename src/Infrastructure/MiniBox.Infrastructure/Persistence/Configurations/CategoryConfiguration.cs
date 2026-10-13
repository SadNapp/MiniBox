using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBox.Domain.Entities.Products;

namespace MiniBox.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.HasKey(category => category.Id);
        entity.Property(category => category.Name).HasMaxLength(100).IsRequired();
        entity.Property(category => category.Description).HasMaxLength(500);
        entity.HasIndex(category => category.Name).IsUnique();
    }
}

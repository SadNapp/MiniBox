using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBox.Domain.Entities.Orders;

namespace MiniBox.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.HasKey(order => order.Id);
        entity.Property(order => order.CustomerName).HasMaxLength(160).IsRequired();
        entity.Property(order => order.CustomerEmail).HasMaxLength(320).IsRequired();
        entity.Property(order => order.DeliveryStreet).HasMaxLength(180).IsRequired();
        entity.Property(order => order.DeliveryCity).HasMaxLength(120).IsRequired();
        entity.Property(order => order.DeliveryPostalCode).HasMaxLength(20).IsRequired();
        entity.Property(order => order.DeliveryCountry).HasMaxLength(80).IsRequired();
        entity.Property(order => order.DeliveryNotes).HasMaxLength(500);
        entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(24);
        entity.Property(order => order.OrderNumber).HasMaxLength(40).IsRequired();
        entity.HasIndex(order => order.OrderNumber).IsUnique();
        entity.HasMany(order => order.Items).WithOne().HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
        entity.Ignore(order => order.TotalPrice);
        entity.HasIndex(order => new { order.UserId, order.CreatedAt });
    }
}

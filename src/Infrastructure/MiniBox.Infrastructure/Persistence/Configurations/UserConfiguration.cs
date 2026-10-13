using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniBox.Domain.Entities.Users;

namespace MiniBox.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(user => user.UserId);
        entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
        entity.HasIndex(user => user.Email).IsUnique();
        entity.Property<string>("NormalizedEmail").HasColumnType("text").HasComputedColumnSql("upper(btrim(\"Email\"))", stored: true);
        entity.HasIndex("NormalizedEmail").IsUnique().HasDatabaseName("IX_Users_Email_Normalized");
        entity.Property(user => user.PasswordHash).HasMaxLength(200).IsRequired();
        entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(24).IsRequired();
        entity.Property(user => user.PhoneNumber).HasMaxLength(40);
        entity.Property(user => user.Address).HasMaxLength(300);
        entity.Property(user => user.City).HasMaxLength(120);
    }
}

using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasIndex(p => new { p.Module, p.Action })
            .IsUnique();
    }
}
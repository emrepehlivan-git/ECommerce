using ECommerce.Application.Common.Constants;
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
            .HasMaxLength(PermissionConsts.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(PermissionConsts.DescriptionMaxLength);

        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(PermissionConsts.ModuleMaxLength);

        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(PermissionConsts.ActionMaxLength);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasIndex(p => new { p.Module, p.Action })
            .IsUnique();
    }
}
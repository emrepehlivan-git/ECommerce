using ECommerce.Application.Features.Roles;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(RoleConsts.NameMaxLength);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(RoleConsts.NameMaxLength);
    }
}
using ECommerce.Application.Features.Users;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.FullName, fullName =>
        {
            fullName.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(UserConsts.FirstNameMaxLength)
                .IsRequired();

            fullName.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(UserConsts.LastNameMaxLength)
                .IsRequired();
        });

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
    }
}
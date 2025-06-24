using ECommerce.Application.Features.UserAddresses;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations;

public sealed class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("user_addresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(UserAddressConsts.LabelMaxLengthValue)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Street)
                .HasColumnName("street")
                .HasMaxLength(UserAddressConsts.StreetMaxLengthValue)
                .IsRequired();

            address.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(UserAddressConsts.CityMaxLengthValue)
                .IsRequired();

            address.Property(x => x.ZipCode)
                .HasColumnName("zip_code")
                .HasMaxLength(UserAddressConsts.ZipCodeMaxLengthValue)
                .IsRequired();

            address.Property(x => x.Country)
                .HasColumnName("country")
                .HasMaxLength(UserAddressConsts.CountryMaxLengthValue)
                .IsRequired();
        });

        builder.HasOne(x => x.User)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.IsDefault })
            .HasDatabaseName("ix_user_addresses_user_id_is_default");
    }
} 
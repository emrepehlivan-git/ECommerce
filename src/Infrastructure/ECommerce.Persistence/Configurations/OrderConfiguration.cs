using ECommerce.Application.Features.UserAddresses;
using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.OwnsOne(o => o.ShippingAddress, sa =>
        {
            sa.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(UserAddressConsts.StreetMaxLengthValue);
            sa.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(UserAddressConsts.CityMaxLengthValue);
            sa.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(UserAddressConsts.ZipCodeMaxLengthValue);
            sa.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(UserAddressConsts.CountryMaxLengthValue);
        });

        builder.OwnsOne(o => o.BillingAddress, ba =>
        {
            ba.Property(a => a.Street).HasColumnName("BillingStreet").HasMaxLength(UserAddressConsts.StreetMaxLengthValue);
            ba.Property(a => a.City).HasColumnName("BillingCity").HasMaxLength(UserAddressConsts.CityMaxLengthValue);
            ba.Property(a => a.ZipCode).HasColumnName("BillingZipCode").HasMaxLength(UserAddressConsts.ZipCodeMaxLengthValue);
            ba.Property(a => a.Country).HasColumnName("BillingCountry").HasMaxLength(UserAddressConsts.CountryMaxLengthValue);
        });

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.Status);
    }
}
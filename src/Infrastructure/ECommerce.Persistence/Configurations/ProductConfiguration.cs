using ECommerce.Application.Features.Products;
using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(ProductConsts.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(ProductConsts.DescriptionMaxLength);

        builder.Property(p => p.Price)
            .HasConversion(
                price => price.Value,
                value => Price.Create(value))
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Stock)
            .WithOne(ps => ps.Product)
            .HasForeignKey<ProductStock>(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.IsActive);
    }
}

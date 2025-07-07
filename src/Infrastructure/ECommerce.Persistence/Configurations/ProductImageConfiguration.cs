using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations;

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.ProductId)
            .IsRequired();

        builder.Property(pi => pi.CloudinaryPublicId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pi => pi.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pi => pi.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(pi => pi.LargeUrl)
            .HasMaxLength(500);

        builder.Property(pi => pi.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(pi => pi.ImageType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pi => pi.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pi => pi.FileSizeBytes)
            .IsRequired();

        builder.Property(pi => pi.AltText)
            .HasMaxLength(250);

        // Relationships
        builder.HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pi => pi.ProductId);
        builder.HasIndex(pi => pi.IsActive);
        builder.HasIndex(pi => new { pi.ProductId, pi.DisplayOrder });
        builder.HasIndex(pi => new { pi.ProductId, pi.ImageType });
        builder.HasIndex(pi => pi.CloudinaryPublicId)
            .IsUnique();
    }
} 
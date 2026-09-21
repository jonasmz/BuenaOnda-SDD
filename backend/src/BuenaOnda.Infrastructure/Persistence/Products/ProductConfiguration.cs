using BuenaOnda.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuenaOnda.Infrastructure.Persistence.Products;

internal sealed class ProductConfiguration :
    IEntityTypeConfiguration<Product>, IEntityTypeConfiguration<SellableOption>, IEntityTypeConfiguration<VariationCharacteristic>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.NormalizedName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.ImageUrl).HasMaxLength(2000);
        builder.Ignore(p => p.HasImage);
        builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => new { p.CategoryId, p.NormalizedName }).IsUnique();
        builder.HasMany(p => p.Options).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Options).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(p => p.Characteristics).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Characteristics).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<VariationCharacteristic> builder)
    {
        builder.ToTable("variation_characteristics");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NormalizedName).IsRequired().HasMaxLength(200);
        builder.HasIndex("ProductId", nameof(VariationCharacteristic.NormalizedName)).IsUnique();
    }

    public void Configure(EntityTypeBuilder<SellableOption> builder)
    {
        builder.ToTable("sellable_options");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.Price).HasPrecision(18, 2);
        builder.Property(o => o.Description).HasMaxLength(1000);
        builder.Property(o => o.ImageUrl).HasMaxLength(2000);
        builder.Property(o => o.Signature).IsRequired().HasMaxLength(2000);
        builder.HasIndex("ProductId", nameof(SellableOption.Signature)).IsUnique();
        builder.Navigation(o => o.Values).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(o => o.Values, values =>
        {
            values.ToTable("option_values");
            values.WithOwner().HasForeignKey("SellableOptionId");
            values.HasKey("SellableOptionId", nameof(OptionValue.CharacteristicId));
            values.Property(v => v.CharacteristicId).ValueGeneratedNever();
            values.Property(v => v.Value).IsRequired().HasMaxLength(500);
            values.Property(v => v.NormalizedValue).IsRequired().HasMaxLength(500);
        });
    }
}

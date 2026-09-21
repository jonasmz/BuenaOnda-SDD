using BuenaOnda.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuenaOnda.Infrastructure.Persistence.Products;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>, IEntityTypeConfiguration<SellableOption>
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
    }

    public void Configure(EntityTypeBuilder<SellableOption> builder)
    {
        builder.ToTable("sellable_options");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.Price).HasPrecision(18, 2);
        builder.Property(o => o.Description).HasMaxLength(1000);
        builder.Property(o => o.ImageUrl).HasMaxLength(2000);
    }
}

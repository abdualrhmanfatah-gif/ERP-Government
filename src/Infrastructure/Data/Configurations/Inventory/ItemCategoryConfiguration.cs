using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
{
    public void Configure(EntityTypeBuilder<ItemCategory> builder)
    {
        builder.ToTable("ItemCategories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.NameEn)
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Breadcrumb)
            .HasMaxLength(1000);

        builder.Property(e => e.TaxClass)
            .HasMaxLength(50);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne(e => e.ParentItemCategory)
            .WithMany()
            .HasForeignKey(e => e.ParentItemCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ParentItemCategoryId);
    }
}

using CatalogService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category");
        builder.HasKey(e => e.CategoryId);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime");
        
        // Унікальні індекси
        builder.HasIndex(e => e.Name)
            .IsUnique();
        
        builder.HasIndex(e => e.Slug)
            .IsUnique();
    }
}
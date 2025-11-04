using CatalogService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Data.Configurations;

public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.ToTable("Hall");
        builder.HasKey(e => e.HallId);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.HallType)
            .HasMaxLength(50);
        
        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("datetime");
        
        // Обмеження
        builder.HasCheckConstraint("CK_Hall_Capacity", "Capacity > 0");
        builder.HasCheckConstraint("CK_Hall_RowCount", "RowCount > 0");
        builder.HasCheckConstraint("CK_Hall_SeatsPerRow", "SeatsPerRow > 0");
        
        // Унікальна назва залу
        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
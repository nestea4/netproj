using CatalogService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Data.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movie");
        builder.HasKey(e => e.MovieId);
        
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(e => e.OriginalTitle)
            .HasMaxLength(255);
        
        builder.Property(e => e.Description)
            .HasMaxLength(2000);
        
        builder.Property(e => e.Director)
            .HasMaxLength(200);
        
        builder.Property(e => e.Rating)
            .HasColumnType("decimal(3,1)");
        
        builder.Property(e => e.PosterUrl)
            .HasMaxLength(500);
        
        builder.Property(e => e.TrailerUrl)
            .HasMaxLength(500);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("datetime");
        
        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);
        
        // Індекси
        builder.HasIndex(e => e.Title);
        builder.HasIndex(e => e.ReleaseDate);
        builder.HasIndex(e => e.IsDeleted);
        
        // Обмеження
        builder.HasCheckConstraint("CK_Movie_DurationMinutes", "DurationMinutes > 0");
        builder.HasCheckConstraint("CK_Movie_Rating", "Rating >= 0 AND Rating <= 10");
    }
}
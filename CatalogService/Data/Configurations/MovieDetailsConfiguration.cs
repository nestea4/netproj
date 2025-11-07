using CatalogService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Data.Configurations;

public class MovieDetailsConfiguration : IEntityTypeConfiguration<MovieDetails>
{
    public void Configure(EntityTypeBuilder<MovieDetails> builder)
    {
        builder.ToTable("MovieDetails");
        builder.HasKey(e => e.MovieDetailsId);
        
        builder.Property(e => e.Country)
            .HasMaxLength(100);
        
        builder.Property(e => e.Language)
            .HasMaxLength(100);
        
        builder.Property(e => e.Budget)
            .HasMaxLength(100);
        
        builder.Property(e => e.BoxOffice)
            .HasMaxLength(100);
        
        builder.Property(e => e.Cast)
            .HasMaxLength(1000);
        
        builder.Property(e => e.AgeRating)
            .HasMaxLength(20);
        
        builder.Property(e => e.Awards)
            .HasMaxLength(500);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime");
        
        // Зв'язок 1:1 з Movie
        builder.HasOne(d => d.Movie)
            .WithOne(m => m.Details)
            .HasForeignKey<MovieDetails>(d => d.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Унікальний індекс для забезпечення 1:1
        builder.HasIndex(e => e.MovieId)
            .IsUnique();
    }
}
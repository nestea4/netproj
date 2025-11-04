using CatalogService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Data.Configurations;

public class MovieCategoryConfiguration : IEntityTypeConfiguration<MovieCategory>
{
    public void Configure(EntityTypeBuilder<MovieCategory> builder)
    {
        builder.ToTable("MovieCategory");
        
        //композитний первинний ключ
        builder.HasKey(mc => new { mc.MovieId, mc.CategoryId });
        
        // Зв'язки M:N
        builder.HasOne(mc => mc.Movie)
            .WithMany(m => m.MovieCategories)
            .HasForeignKey(mc => mc.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(mc => mc.Category)
            .WithMany(c => c.MovieCategories)
            .HasForeignKey(mc => mc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Індекси для швидкого пошуку
        builder.HasIndex(mc => mc.MovieId);
        builder.HasIndex(mc => mc.CategoryId);
    }
}
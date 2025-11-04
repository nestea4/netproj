using CatalogService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Data.Configurations;

public class ShowtimeConfiguration : IEntityTypeConfiguration<Showtime>
{
    public void Configure(EntityTypeBuilder<Showtime> builder)
    {
        builder.ToTable("Showtime");
        builder.HasKey(e => e.ShowtimeId);
        
        builder.Property(e => e.BasePrice)
            .HasColumnType("decimal(10,2)");
        
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);
        
        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(e => e.StartTime)
            .HasColumnType("datetime");
        
        builder.Property(e => e.EndTime)
            .HasColumnType("datetime");
        
        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("datetime");
        
        // Зв'язки 1:N
        builder.HasOne(s => s.Movie)
            .WithMany(m => m.Showtimes)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.Hall)
            .WithMany(h => h.Showtimes)
            .HasForeignKey(s => s.HallId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Обмеження
        builder.HasCheckConstraint("CK_Showtime_Price", "BasePrice >= 0");
        builder.HasCheckConstraint("CK_Showtime_Seats", "AvailableSeats >= 0");
        builder.HasCheckConstraint("CK_Showtime_Times", "EndTime > StartTime");
        
        // Індекси
        builder.HasIndex(e => e.MovieId);
        builder.HasIndex(e => e.HallId);
        builder.HasIndex(e => e.StartTime);
        builder.HasIndex(e => new { e.StartTime, e.IsActive });
        
        // Ігнорування обчислюваних властивостей
        builder.Ignore(e => e.IsPastShowtime);
        builder.Ignore(e => e.DurationMinutes);
    }
}
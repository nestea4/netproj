using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

/// <summary>
/// Database Context для Catalog Service
/// </summary>
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    // DbSets (таблиці)
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieDetails> MovieDetails { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<MovieCategory> MovieCategories { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Movie configuration
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.ToTable("Movie");
            entity.HasKey(e => e.MovieId);
            
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OriginalTitle).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Director).HasMaxLength(200);
            entity.Property(e => e.Rating).HasColumnType("decimal(3,1)");
            entity.Property(e => e.PosterUrl).HasMaxLength(500);
            entity.Property(e => e.TrailerUrl).HasMaxLength(500);
            
            // Простіше: без DEFAULT в БД, використовуємо SaveChanges override
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            // Індекси
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.ReleaseDate);
            entity.HasIndex(e => e.IsDeleted);
        });

        // MovieDetails configuration (1:1 з Movie)
        modelBuilder.Entity<MovieDetails>(entity =>
        {
            entity.ToTable("MovieDetails");
            entity.HasKey(e => e.MovieDetailsId);
            
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Language).HasMaxLength(100);
            entity.Property(e => e.Budget).HasMaxLength(100);
            entity.Property(e => e.BoxOffice).HasMaxLength(100);
            entity.Property(e => e.Cast).HasMaxLength(1000);
            entity.Property(e => e.AgeRating).HasMaxLength(20);
            entity.Property(e => e.Awards).HasMaxLength(500);
            
            // Виправлення datetime
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            
            // 1:1 relationship
            entity.HasOne(d => d.Movie)
                  .WithOne(m => m.Details)
                  .HasForeignKey<MovieDetails>(d => d.MovieId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Унікальний індекс для 1:1
            entity.HasIndex(e => e.MovieId).IsUnique();
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(e => e.CategoryId);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            // Виправлення datetime
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            
            // Унікальність назви та slug
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // MovieCategory configuration (M:N)
        modelBuilder.Entity<MovieCategory>(entity =>
        {
            entity.ToTable("MovieCategory");
            
            // Композитний первинний ключ
            entity.HasKey(mc => new { mc.MovieId, mc.CategoryId });
            
            // Зв'язки
            entity.HasOne(mc => mc.Movie)
                  .WithMany(m => m.MovieCategories)
                  .HasForeignKey(mc => mc.MovieId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(mc => mc.Category)
                  .WithMany(c => c.MovieCategories)
                  .HasForeignKey(mc => mc.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Індекси
            entity.HasIndex(mc => mc.MovieId);
            entity.HasIndex(mc => mc.CategoryId);
        });

        // Hall configuration
        modelBuilder.Entity<Hall>(entity =>
        {
            entity.ToTable("Hall");
            entity.HasKey(e => e.HallId);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.HallType).HasMaxLength(50);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            // Виправлення datetime
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            
            // Обмеження
            entity.HasCheckConstraint("CK_Hall_Capacity", "Capacity > 0");
            entity.HasCheckConstraint("CK_Hall_RowCount", "RowCount > 0");
            
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Showtime configuration
        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.ToTable("Showtime");
            entity.HasKey(e => e.ShowtimeId);
            
            entity.Property(e => e.BasePrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            // Виправлення datetime
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            
            // Зв'язки (1:N)
            entity.HasOne(s => s.Movie)
                  .WithMany(m => m.Showtimes)
                  .HasForeignKey(s => s.MovieId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(s => s.Hall)
                  .WithMany(h => h.Showtimes)
                  .HasForeignKey(s => s.HallId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Обмеження
            entity.HasCheckConstraint("CK_Showtime_Price", "BasePrice >= 0");
            entity.HasCheckConstraint("CK_Showtime_Seats", "AvailableSeats >= 0");
            
            // Індекси
            entity.HasIndex(e => e.MovieId);
            entity.HasIndex(e => e.HallId);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => new { e.StartTime, e.IsActive });
        });
    }

    // Override SaveChanges для автоматичного оновлення аудитних полів
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Movie movie)
            {
                if (entry.State == EntityState.Added)
                {
                    movie.CreatedAt = DateTime.Now;
                }
                movie.UpdatedAt = DateTime.Now;
            }
            else if (entry.Entity is Hall hall)
            {
                if (entry.State == EntityState.Added)
                {
                    hall.CreatedAt = DateTime.Now;
                }
                hall.UpdatedAt = DateTime.Now;
            }
            else if (entry.Entity is Showtime showtime)
            {
                if (entry.State == EntityState.Added)
                {
                    showtime.CreatedAt = DateTime.Now;
                }
                showtime.UpdatedAt = DateTime.Now;
            }
        }
    }
}
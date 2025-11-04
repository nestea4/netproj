using CatalogService.Data.Configurations;
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

    //DbSets (таблиці)
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieDetails> MovieDetails { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<MovieCategory> MovieCategories { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //конфігурації з окремих файлів
        modelBuilder.ApplyConfiguration(new MovieConfiguration());
        modelBuilder.ApplyConfiguration(new MovieDetailsConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new MovieCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new HallConfiguration());
        modelBuilder.ApplyConfiguration(new ShowtimeConfiguration());
    }

    //Override SaveChanges для автоматичного оновлення аудитних полів
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
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

/// <summary>
/// Seeder для початкових даних каталогу
/// </summary>
public static class CatalogDbSeeder
{
    public static async Task SeedAsync(CatalogDbContext context)
    {
        //чи вже є дані
        if (await context.Movies.AnyAsync())
        {
            return; //вже є
        }

        // 1.Categories
        var categories = new List<Category>
        {
            new() { Name = "Action", Description = "Action movies", Slug = "action", CreatedAt = DateTime.Now },
            new() { Name = "Drama", Description = "Drama movies", Slug = "drama", CreatedAt = DateTime.Now },
            new() { Name = "Sci-Fi", Description = "Science Fiction", Slug = "sci-fi", CreatedAt = DateTime.Now },
            new() { Name = "Comedy", Description = "Comedy movies", Slug = "comedy", CreatedAt = DateTime.Now },
            new() { Name = "Thriller", Description = "Thriller movies", Slug = "thriller", CreatedAt = DateTime.Now },
            new() { Name = "Horror", Description = "Horror movies", Slug = "horror", CreatedAt = DateTime.Now }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // 2.Halls
        var halls = new List<Hall>
        {
            new() 
            { 
                Name = "Hall 1", 
                Capacity = 150, 
                RowCount = 10, 
                SeatsPerRow = 15, 
                HallType = "Standard",
                HasWheelchairAccess = true,
                CreatedAt = DateTime.Now 
            },
            new() 
            { 
                Name = "Hall 2", 
                Capacity = 200, 
                RowCount = 12, 
                SeatsPerRow = 17, 
                HallType = "IMAX",
                HasWheelchairAccess = true,
                CreatedAt = DateTime.Now 
            },
            new() 
            { 
                Name = "Hall 3", 
                Capacity = 100, 
                RowCount = 8, 
                SeatsPerRow = 13, 
                HallType = "3D",
                HasWheelchairAccess = false,
                CreatedAt = DateTime.Now 
            },
            new() 
            { 
                Name = "Hall 4", 
                Capacity = 80, 
                RowCount = 8, 
                SeatsPerRow = 10, 
                HallType = "VIP",
                HasWheelchairAccess = true,
                CreatedAt = DateTime.Now 
            }
        };
        context.Halls.AddRange(halls);
        await context.SaveChangesAsync();

        // 3.Movies
        var movie1 = new Movie
        {
            Title = "Dune: Part Two",
            OriginalTitle = "Dune: Part Two",
            Description = "Paul Atreides unites with Chani and the Fremen while seeking revenge against the conspirators who destroyed his family.",
            DurationMinutes = 166,
            ReleaseDate = new DateTime(2024, 3, 1),
            Director = "Denis Villeneuve",
            Rating = 8.7m,
            PosterUrl = "https://example.com/dune2.jpg",
            TrailerUrl = "https://youtube.com/watch?v=dune2",
            CreatedAt = DateTime.Now,
            CreatedBy = "Seeder"
        };

        var movie2 = new Movie
        {
            Title = "Oppenheimer",
            OriginalTitle = "Oppenheimer",
            Description = "The story of American scientist J. Robert Oppenheimer and his role in the development of the atomic bomb.",
            DurationMinutes = 180,
            ReleaseDate = new DateTime(2023, 7, 21),
            Director = "Christopher Nolan",
            Rating = 8.5m,
            PosterUrl = "https://example.com/oppenheimer.jpg",
            TrailerUrl = "https://youtube.com/watch?v=oppenheimer",
            CreatedAt = DateTime.Now,
            CreatedBy = "Seeder"
        };

        var movie3 = new Movie
        {
            Title = "Poor Things",
            OriginalTitle = "Poor Things",
            Description = "The incredible tale of Bella Baxter, a young woman brought back to life by a brilliant scientist.",
            DurationMinutes = 141,
            ReleaseDate = new DateTime(2023, 12, 8),
            Director = "Yorgos Lanthimos",
            Rating = 7.9m,
            PosterUrl = "https://example.com/poorthings.jpg",
            TrailerUrl = "https://youtube.com/watch?v=poorthings",
            CreatedAt = DateTime.Now,
            CreatedBy = "Seeder"
        };

        var movie4 = new Movie
        {
            Title = "Barbie",
            OriginalTitle = "Barbie",
            Description = "Barbie and Ken are having the time of their lives in the colorful and seemingly perfect world of Barbie Land.",
            DurationMinutes = 114,
            ReleaseDate = new DateTime(2023, 7, 21),
            Director = "Greta Gerwig",
            Rating = 7.1m,
            PosterUrl = "https://example.com/barbie.jpg",
            TrailerUrl = "https://youtube.com/watch?v=barbie",
            CreatedAt = DateTime.Now,
            CreatedBy = "Seeder"
        };

        context.Movies.AddRange(movie1, movie2, movie3, movie4);
        await context.SaveChangesAsync();

        // 4.Movie Details (1:1)
        var details = new List<MovieDetails>
        {
            new() 
            { 
                MovieId = movie1.MovieId, 
                Country = "USA", 
                Language = "English", 
                Budget = "$190 million", 
                BoxOffice = "$711 million",
                Cast = "Timothée Chalamet, Zendaya, Rebecca Ferguson",
                AgeRating = "PG-13",
                Awards = "Academy Award for Best Cinematography",
                CreatedAt = DateTime.Now
            },
            new() 
            { 
                MovieId = movie2.MovieId, 
                Country = "USA", 
                Language = "English", 
                Budget = "$100 million", 
                BoxOffice = "$952 million",
                Cast = "Cillian Murphy, Emily Blunt, Matt Damon",
                AgeRating = "R",
                Awards = "Academy Award for Best Picture",
                CreatedAt = DateTime.Now
            },
            new() 
            { 
                MovieId = movie3.MovieId, 
                Country = "UK, USA", 
                Language = "English", 
                Budget = "$35 million", 
                BoxOffice = "$117 million",
                Cast = "Emma Stone, Mark Ruffalo, Willem Dafoe",
                AgeRating = "R",
                Awards = "Golden Lion at Venice Film Festival",
                CreatedAt = DateTime.Now
            },
            new() 
            { 
                MovieId = movie4.MovieId, 
                Country = "USA", 
                Language = "English", 
                Budget = "$145 million", 
                BoxOffice = "$1.446 billion",
                Cast = "Margot Robbie, Ryan Gosling, America Ferrera",
                AgeRating = "PG-13",
                Awards = "Golden Globe for Cinematic Achievement",
                CreatedAt = DateTime.Now
            }
        };
        context.MovieDetails.AddRange(details);
        await context.SaveChangesAsync();

        // 5.Movie-Category relationships (M:N)
        var movieCategories = new List<MovieCategory>
        {
            new() { MovieId = movie1.MovieId, CategoryId = categories[0].CategoryId }, // Dune - Action
            new() { MovieId = movie1.MovieId, CategoryId = categories[2].CategoryId }, // Dune - Sci-Fi
            new() { MovieId = movie2.MovieId, CategoryId = categories[1].CategoryId }, // Oppenheimer - Drama
            new() { MovieId = movie2.MovieId, CategoryId = categories[4].CategoryId }, // Oppenheimer - Thriller
            new() { MovieId = movie3.MovieId, CategoryId = categories[1].CategoryId }, // Poor Things - Drama
            new() { MovieId = movie3.MovieId, CategoryId = categories[3].CategoryId }, // Poor Things - Comedy
            new() { MovieId = movie4.MovieId, CategoryId = categories[3].CategoryId }, // Barbie - Comedy
        };
        context.MovieCategories.AddRange(movieCategories);
        await context.SaveChangesAsync();

        // 6.Showtimes
        var baseDate = DateTime.Now.Date.AddDays(3); //Сеанси через 3 дні
        var showtimes = new List<Showtime>
        {
            //Dune - Hall 1
            new() 
            { 
                MovieId = movie1.MovieId, 
                HallId = halls[0].HallId,
                StartTime = baseDate.AddHours(19),
                EndTime = baseDate.AddHours(19).AddMinutes(166),
                BasePrice = 150.00m,
                AvailableSeats = 150,
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new() 
            { 
                MovieId = movie1.MovieId, 
                HallId = halls[0].HallId,
                StartTime = baseDate.AddHours(22),
                EndTime = baseDate.AddHours(22).AddMinutes(166),
                BasePrice = 150.00m,
                AvailableSeats = 150,
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            
            //Oppenheimer - Hall 2(IMAX)
            new() 
            { 
                MovieId = movie2.MovieId, 
                HallId = halls[1].HallId,
                StartTime = baseDate.AddHours(18),
                EndTime = baseDate.AddHours(18).AddMinutes(180),
                BasePrice = 200.00m,
                AvailableSeats = 200,
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new() 
            { 
                MovieId = movie2.MovieId, 
                HallId = halls[1].HallId,
                StartTime = baseDate.AddHours(21).AddMinutes(30),
                EndTime = baseDate.AddHours(21).AddMinutes(30).AddMinutes(180),
                BasePrice = 200.00m,
                AvailableSeats = 200,
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            
            //Poor Things - Hall 3
            new() 
            { 
                MovieId = movie3.MovieId, 
                HallId = halls[2].HallId,
                StartTime = baseDate.AddHours(20),
                EndTime = baseDate.AddHours(20).AddMinutes(141),
                BasePrice = 160.00m,
                AvailableSeats = 100,
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            
            //Barbie - Hall 4 (VIP)
            new() 
            { 
                MovieId = movie4.MovieId, 
                HallId = halls[3].HallId,
                StartTime = baseDate.AddHours(15).AddMinutes(30),
                EndTime = baseDate.AddHours(15).AddMinutes(30).AddMinutes(114),
                BasePrice = 180.00m,
                AvailableSeats = 80,
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new() 
            { 
                MovieId = movie4.MovieId, 
                HallId = halls[3].HallId,
                StartTime = baseDate.AddHours(18),
                EndTime = baseDate.AddHours(18).AddMinutes(114),
                BasePrice = 180.00m,
                AvailableSeats = 80,
                IsActive = true,
                CreatedAt = DateTime.Now
            },
        };
        context.Showtimes.AddRange(showtimes);
        await context.SaveChangesAsync();

        Console.WriteLine("Catalog database seeded successfully!");
    }
}
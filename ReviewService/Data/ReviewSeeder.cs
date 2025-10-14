using MongoDB.Driver;
using ReviewService.Models;

namespace ReviewService.Data;

/// <summary>
/// Seeder для початкових даних MongoDB
/// </summary>
public static class ReviewSeeder
{
    public static async Task SeedAsync(MongoDbContext context)
    {
        // Перевірка чи вже є дані
        var existingReviews = await context.Reviews.CountDocumentsAsync(FilterDefinition<Review>.Empty);
        if (existingReviews > 0)
        {
            return; // Дані вже є
        }

        // Відгуки
        var reviews = new List<Review>
        {
            new()
            {
                MovieId = 1,
                MovieTitle = "Dune: Part Two",
                UserId = 1,
                UserName = "Ivan Petrenko",
                Title = "A Visual Masterpiece!",
                Content = "Denis Villeneuve has outdone himself. The cinematography is breathtaking, and the story keeps you engaged throughout. Timothée Chalamet and Zendaya deliver powerful performances. A must-watch in IMAX!",
                Rating = 10,
                ReviewType = "Detailed",
                IsVerifiedPurchase = true,
                IsSpoiler = false,
                Tags = new List<string> { "Masterpiece", "Visually Stunning", "Epic" },
                Likes = 42,
                Dislikes = 3,
                ViewCount = 156,
                Comments = new List<Comment>
                {
                    new()
                    {
                        UserId = 2,
                        UserName = "Maria Kovalenko",
                        Content = "Totally agree! The soundtrack by Hans Zimmer was incredible too.",
                        Likes = 8,
                        CreatedAt = DateTime.UtcNow.AddHours(-2)
                    },
                    new()
                    {
                        UserId = 3,
                        UserName = "Oleg Shevchenko",
                        Content = "Best sci-fi movie of the decade!",
                        Likes = 5,
                        Replies = new List<CommentReply>
                        {
                            new()
                            {
                                UserId = 1,
                                UserName = "Ivan Petrenko",
                                Content = "Absolutely! Can't wait for Part Three.",
                                CreatedAt = DateTime.UtcNow.AddHours(-1)
                            }
                        },
                        CreatedAt = DateTime.UtcNow.AddHours(-3)
                    }
                },
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                MovieId = 1,
                MovieTitle = "Dune: Part Two",
                UserId = 2,
                UserName = "Maria Kovalenko",
                Title = "Exceeded all expectations",
                Content = "After the first part, I had high expectations, but this movie blew them away. The battle scenes are intense, and the character development is superb.",
                Rating = 9,
                ReviewType = "Standard",
                IsVerifiedPurchase = true,
                IsSpoiler = false,
                Tags = new List<string> { "Action-packed", "Character Development" },
                Likes = 28,
                Dislikes = 1,
                ViewCount = 89,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                MovieId = 2,
                MovieTitle = "Oppenheimer",
                UserId = 3,
                UserName = "Oleg Shevchenko",
                Title = "A Cinematic Achievement",
                Content = "Christopher Nolan delivers another masterpiece. Cillian Murphy's performance is Oscar-worthy. The non-linear storytelling keeps you thinking long after the credits roll.",
                Rating = 10,
                ReviewType = "Detailed",
                IsVerifiedPurchase = true,
                IsSpoiler = false,
                Tags = new List<string> { "Masterpiece", "Mind-bending", "Historical" },
                Likes = 67,
                Dislikes = 2,
                ViewCount = 234,
                Comments = new List<Comment>
                {
                    new()
                    {
                        UserId = 1,
                        UserName = "Ivan Petrenko",
                        Content = "The way Nolan portrayed the moral dilemma was brilliant.",
                        Likes = 12,
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                },
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                MovieId = 3,
                MovieTitle = "Poor Things",
                UserId = 4,
                UserName = "Anna Melnyk",
                Title = "Weird but Wonderful",
                Content = "This movie is unlike anything I've seen before. Emma Stone's performance is transformative. It's quirky, dark, and visually unique. Not for everyone, but I loved it!",
                Rating = 8,
                ReviewType = "Standard",
                IsVerifiedPurchase = true,
                IsSpoiler = false,
                Tags = new List<string> { "Unique", "Dark Comedy", "Artistic" },
                Likes = 19,
                Dislikes = 5,
                ViewCount = 67,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                MovieId = 4,
                MovieTitle = "Barbie",
                UserId = 5,
                UserName = "Dmytro Bondarenko",
                Title = "Surprisingly Deep and Fun",
                Content = "Went in expecting a light comedy, left thinking about gender roles and society. Margot Robbie and Ryan Gosling have amazing chemistry. The humor is sharp and the message is powerful.",
                Rating = 9,
                ReviewType = "Detailed",
                IsVerifiedPurchase = true,
                IsSpoiler = false,
                Tags = new List<string> { "Funny", "Thought-provoking", "Feel-good" },
                Likes = 51,
                Dislikes = 8,
                ViewCount = 178,
                Comments = new List<Comment>
                {
                    new()
                    {
                        UserId = 2,
                        UserName = "Maria Kovalenko",
                        Content = "The soundtrack is catchy! Still stuck in my head.",
                        Likes = 15,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                },
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        await context.Reviews.InsertManyAsync(reviews);

        // Обговорення
        var discussions = new List<Discussion>
        {
            new()
            {
                MovieId = 1,
                MovieTitle = "Dune: Part Two",
                Topic = "Ending Theories and Part Three Predictions",
                Description = "What do you think will happen in Part Three? Let's discuss Paul's journey and the prophecy.",
                CreatedBy = 1,
                CreatedByName = "Ivan Petrenko",
                Posts = new List<DiscussionPost>
                {
                    new()
                    {
                        UserId = 2,
                        UserName = "Maria Kovalenko",
                        Content = "I think Paul will struggle with his power and try to prevent the holy war.",
                        Likes = 7,
                        CreatedAt = DateTime.UtcNow.AddHours(-5)
                    },
                    new()
                    {
                        UserId = 3,
                        UserName = "Oleg Shevchenko",
                        Content = "The book suggests a much darker path. Can't wait to see how they adapt it!",
                        Likes = 5,
                        CreatedAt = DateTime.UtcNow.AddHours(-3)
                    }
                },
                ParticipantCount = 12,
                ViewCount = 89,
                IsPinned = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                MovieId = 2,
                MovieTitle = "Oppenheimer",
                Topic = "Historical Accuracy Discussion",
                Description = "How accurate was the portrayal of Oppenheimer and the Manhattan Project?",
                CreatedBy = 3,
                CreatedByName = "Oleg Shevchenko",
                Posts = new List<DiscussionPost>
                {
                    new()
                    {
                        UserId = 1,
                        UserName = "Ivan Petrenko",
                        Content = "I did some research and most of it is quite accurate, though some timelines were compressed.",
                        Likes = 9,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                },
                ParticipantCount = 8,
                ViewCount = 56,
                IsPinned = false,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                MovieId = 4,
                MovieTitle = "Barbie",
                Topic = "Best Songs from the Soundtrack",
                Description = "Which songs from Barbie are your favorites? Let's create a playlist!",
                CreatedBy = 5,
                CreatedByName = "Dmytro Bondarenko",
                Posts = new List<DiscussionPost>
                {
                    new()
                    {
                        UserId = 2,
                        UserName = "Maria Kovalenko",
                        Content = "I'm Just Ken is stuck in my head 24/7!",
                        Likes = 18,
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new()
                    {
                        UserId = 4,
                        UserName = "Anna Melnyk",
                        Content = "What Was I Made For by Billie Eilish is so emotional!",
                        Likes = 14,
                        CreatedAt = DateTime.UtcNow.AddHours(-12)
                    }
                },
                ParticipantCount = 15,
                ViewCount = 134,
                IsPinned = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-12)
            }
        };

        await context.Discussions.InsertManyAsync(discussions);

        Console.WriteLine("Review database seeded successfully!");
    }
}
using MongoDB.Driver;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Entities.Disscution;
using ReviewService.Domain.Entities.Review;
using ReviewService.Domain.Enums;

namespace ReviewService.Infrastructure.Data.Seeding;

/// <summary>
/// Seeder для початкових даних MongoDB
/// </summary>
public static class ReviewDataSeeder
{
    public static async Task SeedAsync(IMongoDatabase database)
    {
        var reviewsCollection = database.GetCollection<Review>("Reviews");
        var discussionsCollection = database.GetCollection<Discussion>("Discussions");

        // Перевірка чи вже є дані
        var existingReviews = await reviewsCollection.CountDocumentsAsync(FilterDefinition<Review>.Empty);
        if (existingReviews > 0)
        {
            return; // Вже є дані
        }

        // Створення відгуків через доменні методи
        var reviews = new List<Review>
        {
            Review.Create(
                movieId: 1,
                movieTitle: "Dune: Part Two",
                userId: 1,
                userName: "Ivan Petrenko",
                title: "A Visual Masterpiece!",
                content: "Denis Villeneuve has outdone himself. The cinematography is breathtaking, and the story keeps you engaged throughout. Timothée Chalamet and Zendaya deliver powerful performances. A must-watch in IMAX!",
                rating: 10,
                isSpoiler: false,
                isVerifiedPurchase: true,
                reviewType: ReviewType.Detailed,
                tags: new List<string> { "Masterpiece", "Visually Stunning", "Epic" }
            ),

            Review.Create(
                movieId: 1,
                movieTitle: "Dune: Part Two",
                userId: 2,
                userName: "Maria Kovalenko",
                title: "Exceeded all expectations",
                content: "After the first part, I had high expectations, but this movie blew them away. The battle scenes are intense, and the character development is superb.",
                rating: 9,
                isSpoiler: false,
                isVerifiedPurchase: true,
                reviewType: ReviewType.Standard,
                tags: new List<string> { "Action-packed", "Character Development" }
            ),

            Review.Create(
                movieId: 2,
                movieTitle: "Oppenheimer",
                userId: 3,
                userName: "Oleg Shevchenko",
                title: "A Cinematic Achievement",
                content: "Christopher Nolan delivers another masterpiece. Cillian Murphy's performance is Oscar-worthy. The non-linear storytelling keeps you thinking long after the credits roll.",
                rating: 10,
                isSpoiler: false,
                isVerifiedPurchase: true,
                reviewType: ReviewType.Detailed,
                tags: new List<string> { "Masterpiece", "Mind-bending", "Historical" }
            ),

            Review.Create(
                movieId: 3,
                movieTitle: "Poor Things",
                userId: 4,
                userName: "Anna Melnyk",
                title: "Weird but Wonderful",
                content: "This movie is unlike anything I've seen before. Emma Stone's performance is transformative. It's quirky, dark, and visually unique. Not for everyone, but I loved it!",
                rating: 8,
                isSpoiler: false,
                isVerifiedPurchase: true,
                reviewType: ReviewType.Standard,
                tags: new List<string> { "Unique", "Dark Comedy", "Artistic" }
            ),

            Review.Create(
                movieId: 4,
                movieTitle: "Barbie",
                userId: 5,
                userName: "Dmytro Bondarenko",
                title: "Surprisingly Deep and Fun",
                content: "Went in expecting a light comedy, left thinking about gender roles and society. Margot Robbie and Ryan Gosling have amazing chemistry. The humor is sharp and the message is powerful.",
                rating: 9,
                isSpoiler: false,
                isVerifiedPurchase: true,
                reviewType: ReviewType.Detailed,
                tags: new List<string> { "Funny", "Thought-provoking", "Feel-good" }
            )
        };

        // Додаємо коментарі через доменні методи
        reviews[0].AddComment(2, "Maria Kovalenko", "Totally agree! The soundtrack by Hans Zimmer was incredible too.");
        reviews[0].AddComment(3, "Oleg Shevchenko", "Best sci-fi movie of the decade!");
        
        reviews[2].AddComment(1, "Ivan Petrenko", "The way Nolan portrayed the moral dilemma was brilliant.");
        
        reviews[4].AddComment(2, "Maria Kovalenko", "The soundtrack is catchy! Still stuck in my head.");

        // Додаємо лайки
        for (int i = 0; i < 42; i++) reviews[0].AddLike();
        for (int i = 0; i < 3; i++) reviews[0].AddDislike();
        
        for (int i = 0; i < 28; i++) reviews[1].AddLike();
        reviews[1].AddDislike();
        
        for (int i = 0; i < 67; i++) reviews[2].AddLike();
        for (int i = 0; i < 2; i++) reviews[2].AddDislike();
        
        for (int i = 0; i < 19; i++) reviews[3].AddLike();
        for (int i = 0; i < 5; i++) reviews[3].AddDislike();
        
        for (int i = 0; i < 51; i++) reviews[4].AddLike();
        for (int i = 0; i < 8; i++) reviews[4].AddDislike();

        await reviewsCollection.InsertManyAsync(reviews);

        // Створення обговорень через доменні методи
        var discussions = new List<Discussion>
        {
            Discussion.Create(
                movieId: 1,
                movieTitle: "Dune: Part Two",
                userId: 1,
                userName: "Ivan Petrenko",
                topic: "Ending Theories and Part Three Predictions",
                description: "What do you think will happen in Part Three? Let's discuss Paul's journey and the prophecy."
            ),

            Discussion.Create(
                movieId: 2,
                movieTitle: "Oppenheimer",
                userId: 3,
                userName: "Oleg Shevchenko",
                topic: "Historical Accuracy Discussion",
                description: "How accurate was the portrayal of Oppenheimer and the Manhattan Project?"
            ),

            Discussion.Create(
                movieId: 4,
                movieTitle: "Barbie",
                userId: 5,
                userName: "Dmytro Bondarenko",
                topic: "Best Songs from the Soundtrack",
                description: "Which songs from Barbie are your favorites? Let's create a playlist!"
            )
        };

        // Додаємо пости
        discussions[0].AddPost(2, "Maria Kovalenko", "I think Paul will struggle with his power and try to prevent the holy war.");
        discussions[0].AddPost(3, "Oleg Shevchenko", "The book suggests a much darker path. Can't wait to see how they adapt it!");
        discussions[0].Pin(); // Закріплюємо

        discussions[1].AddPost(1, "Ivan Petrenko", "I did some research and most of it is quite accurate, though some timelines were compressed.");

        discussions[2].AddPost(2, "Maria Kovalenko", "I'm Just Ken is stuck in my head 24/7!");
        discussions[2].AddPost(4, "Anna Melnyk", "What Was I Made For by Billie Eilish is so emotional!");

        await discussionsCollection.InsertManyAsync(discussions);

        Console.WriteLine("Review database seeded successfully!");
        Console.WriteLine($"   - {reviews.Count} reviews added");
        Console.WriteLine($"   - {discussions.Count} discussions added");
    }
}
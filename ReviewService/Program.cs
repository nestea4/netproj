using ReviewService.Configuration;
using ReviewService.Data;
using ReviewService.Data.Repositories;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

// Додавання сервісів
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cinema Review Service API",
        Version = "v1",
        Description = "API для управління відгуками та обговореннями фільмів (Проєкт №3 - MongoDB)"
    });
});

// Реєстрація MongoDB
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()
    ?? throw new InvalidOperationException("MongoDB settings not found");

builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<DiscussionRepository>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database (при старті додатку)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Перевірка підключення та seed даних
        await ReviewSeeder.SeedAsync(context);
        logger.LogInformation("Successfully connected to MongoDB and seeded data");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to MongoDB or seed data");
    }
}

// Налаштування HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Review Service API v1");
        options.RoutePrefix = string.Empty; // Swagger на root URL
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Service = "Cinema Review Service",
    Status = "Running",
    Database = "MongoDB",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0"
}));

app.Run();
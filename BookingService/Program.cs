using BookingService.Data;
using BookingService.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Додавання сервісів до контейнера
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cinema Booking Service API",
        Version = "v1",
        Description = "API для управління бронюванням квитків у кінотеатрі (Проєкт №1 - SQL + ADO.NET & Dapper)"
    });
});

// Реєстрація Database Connection Factory
var connectionString = builder.Configuration.GetConnectionString("BookingDb") 
                       ?? throw new InvalidOperationException("Connection string 'BookingDb' not found");

builder.Services.AddSingleton(new DbConnectionFactory(connectionString));

// Реєстрація Repositories
builder.Services.AddScoped<BookingRepository>();

// CORS (якщо потрібно для frontend)
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

// Налаштування HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking Service API v1");
        options.RoutePrefix = string.Empty; // Swagger на root URL
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Welcome endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Service = "Cinema Booking Service",
    Status = "Running",
    Timestamp = DateTime.Now,
    Version = "1.0.0"
}));

app.Run();
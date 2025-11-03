using BookingService.Dal;
using BookingService.Data;
using BookingService.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

//додати сервісів до контейнера
builder.Services.AddControllers();

//іwagger/OpenAPI
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

//реєстрація Database Connection Factory
var connectionString = builder.Configuration.GetConnectionString("BookingDb") 
                       ?? throw new InvalidOperationException("Connection string 'BookingDb' not found");

builder.Services.AddSingleton(new DbConnectionFactory(connectionString));

//реєстрація Repositories
builder.Services.AddScoped<BookingRepository>();

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

//налаштування HTTP pipeline
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

app.MapGet("/health", () => Results.Ok(new
{
    Service = "Cinema Booking Service",
    Status = "Running",
    Timestamp = DateTime.Now,
    Version = "1.0.0"
}));

app.Run();
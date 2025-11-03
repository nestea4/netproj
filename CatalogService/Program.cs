using CatalogService.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

//додавання сервісів
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cinema Catalog Service API",
        Version = "v1",
        Description = "API для управління каталогом фільмів та сеансів (Проєкт №2 - SQL + EF Core)"
    });
});

//реєстрація DbContext з MySQL
var connectionString = builder.Configuration.GetConnectionString("CatalogDb") 
    ?? throw new InvalidOperationException("Connection string 'CatalogDb' not found");

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//CORS
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

//Seed database(при старті)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    
    try
    {
        //застосувати міграції автоматично
        await context.Database.MigrateAsync();
        
        //іeed даних
        await CatalogDbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

//налаштування HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service API v1");
        options.RoutePrefix = string.Empty; //swagger на root URL
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

//health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Service = "Cinema Catalog Service",
    Status = "Running",
    Timestamp = DateTime.Now,
    Version = "1.0.0"
}));

app.Run();
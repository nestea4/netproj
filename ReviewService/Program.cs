/*using ReviewService.Configuration;
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

app.Run();*/

using System.Reflection;
using FluentValidation;
using MediatR;
using MongoDB.Driver;
using ReviewService.Application.Common.Behaviors;
using ReviewService.Application.Common.Mappings;
using ReviewService.Domain.Interfaces;
using ReviewService.Infrastructure.Data;
using ReviewService.Infrastructure.Data.Repositories;
using ReviewService.Infrastructure.Data.Seeding;
using ReviewService.WebAPI.Middleware;
using ServiceDefaults;
using ServiceDefaults.Middleware;

/*var builder = WebApplication.CreateBuilder(args);

var mongoSettings = builder.Configuration
    .GetSection("MongoDbSettings")
    .Get<MongoDbSettings>() 
    ?? throw new InvalidOperationException("MongoDB settings not found");

builder.Services.AddSingleton(mongoSettings);

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<MongoDbSettings>();
    var context = new MongoDbContext(settings.ConnectionString, settings.DatabaseName);
    return context.Database;
});

builder.Services.AddScoped<IReviewRepository, MongoReviewRepository>();
builder.Services.AddScoped<IDiscussionRepository, MongoDiscussionRepository>();

//MediatR з усіма handlers 
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ReviewService.Application.Commands.Reviews.CreateReviewCommand).Assembly);
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

builder.Services.AddValidatorsFromAssembly(
    typeof(ReviewService.Application.Validators.CreateReviewCommandValidator).Assembly);

builder.Services.AddAutoMapper(typeof(ReviewMappingProfile).Assembly);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cinema Review Service API - Clean Architecture",
        Version = "v1",
        Description = "ASP.NET Core Web API з Clean Architecture, CQRS, MediatR, MongoDB"
    });

    //XML comments для Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();


// 1.Global Exception Handler
app.UseGlobalExceptionHandler();

// 2.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Review Service API v1");
        options.RoutePrefix = string.Empty; // Swagger на root URL
    });
}

// 3.HTTPS Redirection
app.UseHttpsRedirection();

// 4.CORS
app.UseCors("AllowAll");

// 5.
app.UseAuthorization();

// 6.Controllers
app.MapControllers();

// 7.Health Checks
app.MapHealthChecks("/health");

app.Logger.LogInformation("Cinema Review Service starting...");
app.Logger.LogInformation("MongoDB: {ConnectionString}", mongoSettings.ConnectionString);
app.Logger.LogInformation("Database: {DatabaseName}", mongoSettings.DatabaseName);

using (var scope = app.Services.CreateScope())
{
    try
    {
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        
        //Test connection
        var collections = database.ListCollectionNames().ToList();
        
        app.Logger.LogInformation("Successfully connected to MongoDB");
        app.Logger.LogInformation("Found {Count} collections", collections.Count);
        
        //await ReviewDataSeeder.SeedAsync(database);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to connect to MongoDB");
        throw;
    }
}

app.Run();*/

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var mongoSettings = builder.Configuration
    .GetSection("MongoDbSettings")
    .Get<MongoDbSettings>() 
    ?? throw new InvalidOperationException("MongoDB settings not found");

//override з Aspire connection string якщо доступний
var aspireConnectionString = builder.Configuration.GetConnectionString("ReviewDb");
if (!string.IsNullOrEmpty(aspireConnectionString))
{
    mongoSettings.ConnectionString = aspireConnectionString;
}

builder.Services.AddSingleton(mongoSettings);

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<MongoDbSettings>();
    var context = new MongoDbContext(settings.ConnectionString, settings.DatabaseName);
    return context.Database;
});

builder.Services.AddScoped<IReviewRepository, MongoReviewRepository>();
builder.Services.AddScoped<IDiscussionRepository, MongoDiscussionRepository>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(ReviewService.Application.Commands.Reviews.CreateReviewCommand).Assembly);
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

builder.Services.AddValidatorsFromAssembly(
    typeof(ReviewService.Application.Validators.CreateReviewCommandValidator).Assembly);

builder.Services.AddAutoMapper(typeof(ReviewMappingProfile).Assembly);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cinema Review Service API",
        Version = "v1",
        Description = "Clean Architecture + CQRS + MediatR + MongoDB + Aspire"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();


// 1. CorrelationId Middleware
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Global Exception Handler
app.UseGlobalExceptionHandler();

// 3. Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Review Service API v1");
        options.RoutePrefix = string.Empty;
    });
}

// 4.
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 5. ServiceDefaults endpoints 
app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        if (app.Environment.IsDevelopment())
        {
            app.Logger.LogWarning("Development mode: Clearing and reseeding database...");
            await MongoDbMigration.ClearAllDataAsync(database, app.Logger);
            
            await ReviewDataSeeder.SeedAsync(database);
        }
        
        var collections = database.ListCollectionNames().ToList();
        
        app.Logger.LogInformation("Successfully connected to MongoDB");
        app.Logger.LogInformation("Found {Count} collections", collections.Count);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to connect to MongoDB");
        throw;
    }
}

app.MapGet("/api/info", () => Results.Ok(new
{
    Service = "Cinema Review Service",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Architecture = new
    {
        Pattern = "Clean Architecture",
        CQRS = "MediatR",
        Database = "MongoDB",
        Validation = "FluentValidation"
    },
    Observability = new
    {
        Logging = "Serilog (Structured JSON)",
        Tracing = "OpenTelemetry + MongoDB instrumentation",
        CorrelationId = "Enabled"
    }
}));

app.Run();
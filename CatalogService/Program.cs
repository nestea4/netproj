/*using CatalogService.Data;
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

app.Run();*/


using CatalogService.Data;
using CatalogService.Middleware;
using CatalogService.Services;
using CatalogService.Services.Interfaces;
using CatalogService.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Text.Encodings.Web;
using CatalogService.Repositories;
using CatalogService.Repositories.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using ServiceDefaults;
using ServiceDefaults.Middleware;

/*// Налаштування Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CatalogService")
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/catalog-service-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Catalog Service");

    var builder = WebApplication.CreateBuilder(args);

    //Serilog як основний провайдер логування
    builder.Host.UseSerilog();

    //додаю контролерів з налаштуваннями JSON
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    //FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    //AutoMapper
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    //swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Cinema Catalog Service API",
            Version = "v1",
            Description = "RESTful API для управління каталогом фільмів, сеансів та категорій (Проєкт №2 - EF Core + Code First)"
        });

        //gпідключення XML-коментарів для документації
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        //налаштування схем для ProblemDetails
        options.MapType<ProblemDetails>(() => new Microsoft.OpenApi.Models.OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSchema>
            {
                ["type"] = new() { Type = "string" },
                ["title"] = new() { Type = "string" },
                ["status"] = new() { Type = "integer" },
                ["detail"] = new() { Type = "string" },
                ["instance"] = new() { Type = "string" }
            }
        });
    });

    //реєстрація DbContext з MySQL
    var connectionString = builder.Configuration.GetConnectionString("CatalogDb")
        ?? throw new InvalidOperationException("Connection string 'CatalogDb' not found");

    builder.Services.AddDbContext<CatalogDbContext>(options =>
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    //реєстрація репозиторіїв
    builder.Services.AddScoped<IMovieRepository, MovieRepository>();
    builder.Services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

    //реєстрація Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    //реєстрація бізнес-сервісів
    builder.Services.AddScoped<IMovieService, MovieService>();
    builder.Services.AddScoped<IShowtimeService, ShowtimeService>();
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
    
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<CatalogDbContext>("database");

    var app = builder.Build();

    //міграція та Seeding при старті
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<CatalogDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            logger.LogInformation("Seeding database...");
            await CatalogDbSeeder.SeedAsync(context);
            logger.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database");
            throw;
        }
    }

    // Middleware pipeline

    // 1.Exception handling (має бути першим)
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // 2.request logging
    app.UseMiddleware<RequestLoggingMiddleware>();

    // 3.Swagger (тільки в Development)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service API v1");
            options.RoutePrefix = string.Empty; // Swagger на root URL
            options.DocumentTitle = "Cinema Catalog API";
        });
    }

    // 4.HTTPS Redirection
    app.UseHttpsRedirection();

    // 5.CORS
    app.UseCors("AllowAll");

    // 6.Authorization (якщо буде потрібно)
    app.UseAuthorization();

    // 7.Controllers
    app.MapControllers();

    // 8.Health checks
    app.MapHealthChecks("/health");

    //додатковий endpoint для детальної інформації про сервіс
    app.MapGet("/api/info", () =>
    {
        return Results.Ok(new
        {
            Service = "Cinema Catalog Service",
            Version = "1.0.0",
            Environment = app.Environment.EnvironmentName,
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Features = new[]
            {
                "Movies Management",
                "Showtimes Management",
                "Categories",
                "Filtering & Sorting",
                "Pagination"
            }
        });
    }).WithName("ServiceInfo")
      .WithTags("Service")
      .Produces(200);

    Log.Information("Catalog Service started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}*/

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Cinema Catalog Service API",
        Version = "v1",
        Description = "RESTful API для управління каталогом фільмів (EF Core + Aspire)"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.MapType<ProblemDetails>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSchema>
        {
            ["type"] = new() { Type = "string" },
            ["title"] = new() { Type = "string" },
            ["status"] = new() { Type = "integer" },
            ["detail"] = new() { Type = "string" },
            ["instance"] = new() { Type = "string" }
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("CatalogDb")
    ?? throw new InvalidOperationException("Connection string 'CatalogDb' not found");
connectionString = CleanConnectionStringForMySql(connectionString);

builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IShowtimeService, ShowtimeService>();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<CatalogDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");

        logger.LogInformation("Seeding database...");
        await CatalogDbSeeder.SeedAsync(context);
        logger.LogInformation("Database seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

// 1. CorrelationId Middleware 
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 3. Request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// 4. Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service API v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "Cinema Catalog API";
    });
}

// 5. 
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 6. ServiceDefaults endpoints 
app.MapDefaultEndpoints();

// 7. Service info
app.MapGet("/api/info", () =>
{
    return Results.Ok(new
    {
        Service = "Cinema Catalog Service",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Status = "Running",
        Timestamp = DateTime.UtcNow,
        Features = new[]
        {
            "Movies Management",
            "Showtimes Management",
            "Categories",
            "Filtering & Sorting",
            "Pagination"
        },
        Observability = new
        {
            Logging = "Serilog (Structured JSON)",
            Tracing = "OpenTelemetry + EF Core instrumentation",
            CorrelationId = "Enabled"
        }
    });
}).WithName("ServiceInfo")
  .WithTags("Service")
  .Produces(200);

static string CleanConnectionStringForMySql(string connectionString)
{
    var sqlServerParams = new[] 
    { 
        "TrustServerCertificate", 
        "Encrypt",
        "MultipleActiveResultSets",
        "Integrated Security"
    };
    
    var parts = connectionString.Split(';')
        .Where(part => !sqlServerParams.Any(param => 
            part.Trim().StartsWith(param, StringComparison.OrdinalIgnoreCase)))
        .ToList();
    
    return string.Join(";", parts);
}
app.Run();
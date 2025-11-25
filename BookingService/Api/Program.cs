using BookingService.Bll.Interfaces;
using BookingService.Bll.Profiles;
using BookingService.Bll.Services;
using BookingService.Dal;
using BookingService.Dal.Interfaces;
using BookingService.Dal.Repositories;
using BookingService.Dal.UnitOfWork;
using Microsoft.OpenApi.Models;

/*var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookingDb") 
    ?? throw new InvalidOperationException("Connection string 'BookingDb' not found");

//DbConnectionFactory
builder.Services.AddSingleton(new DbConnectionFactory(connectionString));

//Repositories
//1 репозиторій на чистому ADO.NET
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

//2+ репозиторії на ADO.NET + Dapper
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

//Unit of Work (координує транзакції в межах одного запиту)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Сервіси (Scoped)
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBookingService, BookingService.Bll.Services.BookingService>();

//AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfiles).Assembly);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cinema Booking Service API",
        Version = "v1",
        Description = "API для управління бронюванням квитків у кінотеатрі. " +
                      "Тришарова архітектура: DAL (ADO.NET + Dapper) → BLL → API. " +
                      "Unit of Work, транзакції, асинхронність, параметризовані запити.",
        Contact = new OpenApiContact
        {
            Name = "Booking Service Team",
            Email = "support@bookingservice.com"
        }
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
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

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking Service API v1");
        options.RoutePrefix = string.Empty; //Swagger на root URL
        options.DocumentTitle = "Booking Service API";
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
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Architecture = new
    {
        DAL = "ADO.NET + Dapper",
        BLL = "Services with DTO mapping",
        API = "ASP.NET Core Controllers",
        Patterns = new[] { "Repository", "Unit of Work", "Dependency Injection" }
    }
}));

app.Run();*/
using BookingService.Bll.Interfaces;
using BookingService.Bll.Profiles;
using BookingService.Bll.Services;
using BookingService.Dal;
using BookingService.Dal.Interfaces;
using BookingService.Dal.Repositories;
using BookingService.Dal.UnitOfWork;
using Microsoft.OpenApi.Models;
using ServiceDefaults; // ДОДАНО!
using ServiceDefaults.Middleware; // ДОДАНО!

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("BookingDb") 
    ?? throw new InvalidOperationException("Connection string 'BookingDb' not found");

connectionString = CleanConnectionStringForMySql(connectionString);

builder.Services.AddSingleton(new DbConnectionFactory(connectionString));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBookingService, BookingService.Bll.Services.BookingService>();

builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfiles).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cinema Booking Service API",
        Version = "v1",
        Description = "API для управління бронюванням квитків (ADO.NET + Dapper + Aspire)"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
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

// HTTP context (для CorrelationId)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();


// MIDDLEWARE PIPELINE
// 1. CorrelationId Middleware 
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking Service API v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "Booking Service API";
    });
}

// 3
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 4. ServiceDefaults endpoints (health, etc.) 
app.MapDefaultEndpoints();

// 5. 
app.MapGet("/api/info", () => Results.Ok(new
{
    Service = "Cinema Booking Service",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Architecture = new
    {
        DAL = "ADO.NET + Dapper",
        BLL = "Services with DTO mapping",
        API = "ASP.NET Core Controllers",
        Patterns = new[] { "Repository", "Unit of Work", "Dependency Injection" }
    },
    Observability = new
    {
        Logging = "Serilog (Structured JSON)",
        Tracing = "OpenTelemetry",
        CorrelationId = "Enabled"
    }
}));

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

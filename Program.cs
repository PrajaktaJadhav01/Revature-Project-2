using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure;
using CustomerManagementSystem.Infrastructure.Persistence;
using CustomerManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Database
var defaultConnectionString = configuration.GetConnectionString("DefaultConnection") ??
                              "Server=(localdb)\\MSSQLLocalDB;Database=CustomerManagementDb;Trusted_Connection=True;TrustServerCertificate=True;";
var localDbConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CustomerManagementDb;Trusted_Connection=True;TrustServerCertificate=True;";
var connectionStringToUse = defaultConnectionString;
var isUsingInMemoryDatabase = false;

async Task<bool> CanConnectSqlServerAsync(string connectionString)
{
    try
    {
        await using var sqlConnection = new SqlConnection(connectionString);
        await sqlConnection.OpenAsync();
        await sqlConnection.CloseAsync();
        return true;
    }
    catch
    {
        return false;
    }
}

if (!await CanConnectSqlServerAsync(connectionStringToUse))
{
    Console.WriteLine("[Warning] Unable to connect to DefaultConnection. Trying LocalDB fallback...");
    connectionStringToUse = localDbConnectionString;

    if (!await CanConnectSqlServerAsync(connectionStringToUse))
    {
        Console.WriteLine("[Warning] Unable to connect to LocalDB. Using in-memory database fallback.");
        isUsingInMemoryDatabase = true;
    }
    else
    {
        Console.WriteLine("[Info] Connected to LocalDB fallback.");
    }
}

string MaskConnectionString(string connectionString)
{
    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (!string.IsNullOrWhiteSpace(builder.Password)) builder.Password = "*****";
        if (!string.IsNullOrWhiteSpace(builder.UserID)) builder.UserID = builder.UserID;
        return builder.ToString();
    }
    catch
    {
        return connectionString;
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (isUsingInMemoryDatabase)
    {
        options.UseInMemoryDatabase("CustomerManagementSystemInMemory");
    }
    else
    {
        options.UseSqlServer(connectionStringToUse, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        });
    }
});

// Caching (Redis via IDistributedCache) with safe fallback
var redisConnection = configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Authentication / JWT
var jwtSection = configuration.GetSection("Jwt");
var jwtKey = jwtSection["Secret"] ??
             "THIS_IS_A_DEV_ONLY_JWT_SECRET_KEY_CHANGE_IN_PRODUCTION_1234567890";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "CustomerManagementSystem",
            ValidAudience = jwtSection["Audience"] ?? "CustomerManagementSystem.Frontend",
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// Application services
builder.Services.AddInfrastructureServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var dbReady = false;
var dbStatusMessage = "Database connection not established.";

// Ensure database exists / can run without manual migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Using DB connection: {ConnectionString}", isUsingInMemoryDatabase ? "InMemory" : MaskConnectionString(connectionStringToUse));

    if (isUsingInMemoryDatabase)
    {
        dbReady = true;
        dbStatusMessage = "Using in-memory database (no external SQL Server required).";
        logger.LogInformation("In-memory database activated.");
    }
    else
    {
        const int maxAttempts = 30;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting SQL Server connection (try {Attempt}/{MaxAttempts})", attempt, maxAttempts);

                if (await db.Database.CanConnectAsync())
                {
                    logger.LogInformation("Connected to database successfully.");
                    dbReady = true;
                    dbStatusMessage = "Connected";
                    break;
                }

                dbStatusMessage = "Database unreachable.";
                logger.LogWarning("Database is not reachable at attempt {Attempt}.", attempt);
            }
            catch (Exception ex)
            {
                dbStatusMessage = ex.Message;
                logger.LogWarning(ex, "Database connection attempt failed at attempt {Attempt}.", attempt);
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        if (!dbReady)
        {
            logger.LogError("Cannot connect to SQL Server after {MaxAttempts} attempts, and no in-memory fallback is enabled.", maxAttempts);
        }
    }

    if (dbReady)
    {
        try
        {
            if (!isUsingInMemoryDatabase)
            {
                await db.Database.MigrateAsync();
            }

            await db.Database.EnsureCreatedAsync();

            var hasUsers = false;
            try
            {
                hasUsers = await db.Users.AnyAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Users table check failed, recreating database schema.");
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                hasUsers = false;
            }

            if (!hasUsers)
            {
                db.Users.AddRange(
                    new AppUser
                    {
                        Username = "admin",
                        PasswordHash = HashPassword("Admin@123"),
                        Role = UserRole.Admin
                    },
                    new AppUser
                    {
                        Username = "manager",
                        PasswordHash = HashPassword("Manager@123"),
                        Role = UserRole.SalesManager
                    },
                    new AppUser
                    {
                        Username = "rep",
                        PasswordHash = HashPassword("Rep@123"),
                        Role = UserRole.SalesRep,
                        AssignedSalesRepId = 1
                    }
                );

                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            dbStatusMessage = ex.Message;
            logger.LogError(ex, "Error applying database migrations or seeding data.");
            dbReady = false;
        }
    }
    else
    {
        logger.LogError("Cannot initialize database. Starting in degraded mode. Status: {StatusMessage}", dbStatusMessage);
    }
}

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Incoming HTTP {Method} {Path}", context.Request.Method, context.Request.Path);

    try
    {
        await next();
        logger.LogInformation("HTTP {Method} {Path} responded {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        throw;
    }
});

app.MapGet("/health", async (AppDbContext db) =>
{
    var status = dbReady ? "healthy" : "unhealthy";
    var dbConnected = false;
    var detail = dbStatusMessage;

    if (dbReady)
    {
        try
        {
            dbConnected = await db.Database.CanConnectAsync();
            status = dbConnected ? "healthy" : "unhealthy";
            detail = dbConnected ? "Database connected" : "Database connection lost";
        }
        catch (Exception ex)
        {
            status = "unhealthy";
            detail = ex.Message;
        }
    }

    var code = status == "healthy" ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
    return Results.Json(new
    {
        app = "OK",
        database = status,
        dbConnected,
        detail,
        time = DateTime.UtcNow
    }, statusCode: code);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception in API middleware");

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var errorObj = new { message = exception?.Message ?? "Unexpected server error" };
        await context.Response.WriteAsJsonAsync(errorObj);
    });
});

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

static string HashPassword(string password)
{
    const int iterations = 100_000;
    var salt = RandomNumberGenerator.GetBytes(16);
    using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
    var hash = pbkdf2.GetBytes(32);
    return $"pbkdf2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
}

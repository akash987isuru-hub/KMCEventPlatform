using System.Text;
using System.Text.Json.Serialization;
using KMC.Api.Data;
using KMC.Api.Entities;
using KMC.Api.Interfaces;
using KMC.Api.Mapping;
using KMC.Api.Middleware;
using KMC.Api.OpenApi;
using KMC.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(
                namingPolicy: null,
                allowIntegerValues: false));
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(
            context.ModelState)
        {
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://httpstatuses.com/400",
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problemDetails);
    };
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "The DefaultConnection connection string is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IPartnerEventService, PartnerEventService>();
builder.Services.AddScoped<IPartnerBookingService, PartnerBookingService>();
builder.Services.AddScoped<
    IRegistrationService,
    RegistrationService>();

builder.Services.AddHttpClient<
    IOrganizerPartnerBookingClient,
    OrganizerPartnerBookingClient>(client =>
{
    var organizerApiUrl = builder.Configuration["OrganizerIntegration:BaseUrl"]
        ?? throw new InvalidOperationException(
            "OrganizerIntegration:BaseUrl is missing.");

    client.BaseAddress = new Uri(organizerApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("KMC.Api/PartnerBooking");
});

builder.Services.AddHttpClient<
    IOrganizerPartnerEventClient,
    OrganizerPartnerEventClient>(client =>
{
    var organizerApiUrl = builder.Configuration["OrganizerIntegration:BaseUrl"]
        ?? throw new InvalidOperationException(
            "OrganizerIntegration:BaseUrl is missing.");

    client.BaseAddress = new Uri(organizerApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("KMC.Api/PartnerEventManagement");
});

builder.Services.AddAutoMapper(
    configuration => { },
    typeof(EventMappingProfile));

var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException(
        "JWT key is missing. Configure Jwt:Key using user secrets or an environment variable.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT key must contain at least 32 UTF-8 bytes.");
}

var jwtIssuer = jwtSection["Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is missing from configuration.");

var jwtAudience = jwtSection["Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is missing from configuration.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .GetChildren()
    .Select(item => item.Value)
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Cast<string>()
    .ToArray();

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ExternalClients", policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info = new()
            {
                Title = "KMC Event Platform API",
                Version = "v1",
                Description =
                    "Service-oriented API for Kandy Municipal Council event management, participant registration and public event discovery."
            };

            return Task.CompletedTask;
        });

    options.AddDocumentTransformer<
        BearerSecuritySchemeTransformer>();

    options.AddOperationTransformer<
        BearerSecurityRequirementTransformer>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("KMC Event Platform API")
            .AddDocument(
                "v1",
                "KMC Event Platform API")
            .AddPreferredSecuritySchemes("Bearer")
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);
    });
}

var applyMigrations = bool.TryParse(
    builder.Configuration["Database:ApplyMigrationsOnStartup"],
    out var configuredApplyMigrations) &&
    configuredApplyMigrations;

if (applyMigrations)
{
    await ApplyMigrationsWithRetryAsync(app);
}

var useHttpsRedirection =
    !bool.TryParse(
        builder.Configuration["HttpsRedirection:Enabled"],
        out var configuredHttpsRedirection) ||
    configuredHttpsRedirection;

if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}

if (allowedOrigins.Length > 0)
{
    app.UseCors("ExternalClients");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.MapGet("/", ApiRoot)
    .AllowAnonymous()
    .ExcludeFromDescription();

await app.RunAsync();

static IResult ApiRoot(IHostEnvironment environment)
{
    if (environment.IsDevelopment())
    {
        return Results.Redirect("/scalar/v1");
    }

    return Results.Ok(new
    {
        name = "KMC Event Platform API",
        version = "v1",
        status = "running",
        health = "/health"
    });
}

static async Task ApplyMigrationsWithRetryAsync(WebApplication app)
{
    const int maximumAttempts = 10;
    var logger = app.Logger;

    for (var attempt = 1; attempt <= maximumAttempts; attempt++)
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
            return;
        }
        catch (Exception exception) when (attempt < maximumAttempts)
        {
            logger.LogWarning(
                exception,
                "Database migration attempt {Attempt}/{MaximumAttempts} failed. Retrying in 5 seconds.",
                attempt,
                maximumAttempts);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    await using var finalScope = app.Services.CreateAsyncScope();
    var finalDbContext = finalScope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await finalDbContext.Database.MigrateAsync();
}

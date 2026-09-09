using Microsoft.EntityFrameworkCore;
using Organizer.Api.Data;
using Organizer.Api.Interfaces;
using Organizer.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<OrganizerDbContext>(options =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "The Organizer database connection string is missing.");

    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<
    IOrganizerEventService,
    OrganizerEventService>();

builder.Services.AddScoped<
    IOrganizerBookingService,
    OrganizerBookingService>();

builder.Services.AddHttpClient<
    IKmcPartnerEventClient,
    KmcPartnerEventClient>(client =>
{
    var baseUrl = builder.Configuration["KmcApi:BaseUrl"]
        ?? throw new InvalidOperationException(
            "KMC API base URL is missing.");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "SparklingEventsKandy.Api/1.0");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sparkling Events Kandy API");
    });
}
else
{
    app.UseHsts();
}

var applyMigrations =
    !bool.TryParse(
        builder.Configuration["Database:ApplyMigrationsOnStartup"],
        out var configuredApplyMigrations)
        ? app.Environment.IsDevelopment()
        : configuredApplyMigrations;

if (applyMigrations)
{
    await ApplyMigrationsWithRetryAsync(app);
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1"))
    .ExcludeFromDescription();

await app.RunAsync();

static async Task ApplyMigrationsWithRetryAsync(WebApplication app)
{
    const int maximumAttempts = 5;

    for (var attempt = 1; attempt <= maximumAttempts; attempt++)
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<OrganizerDbContext>();

            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation(
                "Organizer database migrations applied successfully.");
            return;
        }
        catch (Exception exception) when (attempt < maximumAttempts)
        {
            app.Logger.LogWarning(
                exception,
                "Organizer database migration attempt {Attempt}/{MaximumAttempts} failed. Retrying in 2 seconds.",
                attempt,
                maximumAttempts);

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    await using var finalScope = app.Services.CreateAsyncScope();
    var finalDbContext = finalScope.ServiceProvider
        .GetRequiredService<OrganizerDbContext>();

    await finalDbContext.Database.MigrateAsync();
}

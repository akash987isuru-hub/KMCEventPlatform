using KMC.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/About");
    options.Conventions.AllowAnonymousToPage("/Status");
    options.Conventions.AllowAnonymousToFolder("/Events");
    options.Conventions.AllowAnonymousToPage("/PartnerEvents/Details");
    options.Conventions.AllowAnonymousToPage("/PartnerEvents/Ticket");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AuthorizeFolder("/Participant", "ParticipantOnly");
    options.Conventions.AuthorizeFolder("/Registrations", "ParticipantOnly");
    options.Conventions.AuthorizeFolder("/Checkout", "ParticipantOnly");
    options.Conventions.AuthorizeFolder("/Organizer", "OrganizerOnly");
});

builder.Services.AddDistributedMemoryCache();

var cookieSecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.Name = ".KMC.Web.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
        options.Cookie.Name = ".KMC.Web.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = cookieSecurePolicy;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OrganizerOnly", policy =>
        policy.RequireRole("Organizer"));

    options.AddPolicy("ParticipantOnly", policy =>
        policy.RequireRole("Participant"));
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".KMC.Web.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services.AddHttpClient<IKmcApiClient, KmcApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException(
            "API base URL is missing from appsettings.json.");

    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var apiBaseAddress))
    {
        throw new InvalidOperationException(
            "ApiSettings:BaseUrl must be a valid absolute URL.");
    }

    client.BaseAddress = apiBaseAddress;
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("KMC.Web/1.0");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Status", "?code={0}");

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] =
        "strict-origin-when-cross-origin";

    await next();
});

var useHttpsRedirection =
    !bool.TryParse(
        builder.Configuration["HttpsRedirection:Enabled"],
        out var configuredHttpsRedirection) ||
    configuredHttpsRedirection;

if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

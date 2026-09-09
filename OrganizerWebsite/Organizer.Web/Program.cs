using Organizer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthorization();

builder.Services.AddHttpClient<IOrganizerApiClient, OrganizerApiClient>(client =>
{
    var baseUrl = builder.Configuration["OrganizerApi:BaseUrl"]
        ?? throw new InvalidOperationException(
            "OrganizerApi:BaseUrl is missing from appsettings.json.");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SparklingEventsKandy.Web/1.0");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

using Microsoft.EntityFrameworkCore;
using RealEase.Persistence.Context;
using RealEase.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddServerSideBlazor();

var apiBaseUrl = (builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7012/").TrimEnd('/') + "/";
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri))
{
    throw new InvalidOperationException("Api:BaseUrl debe ser una URL absoluta de la API.");
}

builder.Services.AddHttpClient<RealEaseApiClient>(client =>
{
    client.BaseAddress = apiUri;
    client.Timeout = TimeSpan.FromSeconds(15);
});

var webConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RealEaseDbContext>(options =>
    options.UseNpgsql(webConnectionString ?? string.Empty).UseSnakeCaseNamingConvention());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapBlazorHub();

app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Npgsql;
using RealEase.Persistence.Context;
using RealEase.Domain.Models;
using RealEase.Web.Security;
using RealEase.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = ".RealEase.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.SlidingExpiration = false;
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.Redirect(options.AccessDeniedPath);
            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = async context =>
        {
            var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionRole = context.Principal?.FindFirstValue(ClaimTypes.Role);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<RealEaseDbContext>();
            var user = await dbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Id == userId, context.HttpContext.RequestAborted);
            var currentRole = UserRoles.Normalize(user?.Role);
            if (user is null || !user.IsActive || currentRole is null || currentRole != sessionRole)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization();

var apiBaseUrl = (builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7012/").TrimEnd('/') + "/";
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri))
{
    throw new InvalidOperationException("Api:BaseUrl debe ser una URL absoluta de la API.");
}

builder.Services.AddTransient<UserAccessTokenHandler>();
builder.Services.AddHttpClient<RealEaseApiClient>(client =>
{
    client.BaseAddress = apiUri;
    client.Timeout = TimeSpan.FromSeconds(15);
}).AddHttpMessageHandler<UserAccessTokenHandler>();

var webConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(webConnectionString))
{
    throw new InvalidOperationException(
        "Falta ConnectionStrings:DefaultConnection. Configura la variable de entorno " +
        "ConnectionStrings__DefaultConnection con la cadena PostgreSQL de Supabase.");
}

try
{
    _ = new NpgsqlConnectionStringBuilder(webConnectionString);
}
catch (ArgumentException exception)
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection no es una cadena de conexión PostgreSQL válida. " +
        "Usa el formato Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require.",
        exception);
}

builder.Services.AddDbContext<RealEaseDbContext>(options =>
    options.UseNpgsql(webConnectionString).UseSnakeCaseNamingConvention());

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapBlazorHub();

app.Run();

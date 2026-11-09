using Microsoft.EntityFrameworkCore;
using Supabase;
using RealEase.Infrastructure.Core;
using RealEase.Infrastructure.Repositories;
using RealEase.Application.Services;
using RealEase.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACION DE BASE DE DATOS (EF CORE + SUPABASE DB)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta ConnectionStrings:DefaultConnection. Configura la variable de entorno " +
        "ConnectionStrings__DefaultConnection con la cadena PostgreSQL de Supabase.");
}

builder.Services.AddDbContext<RealEaseDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention()); // Convierte Id->id, ContractId->contract_id, etc.

// ==========================================
// 2. CONFIGURACION DE CLIENTE SUPABASE (AUTH / STORAGE / REALTIME)
// ==========================================
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];
if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseKey))
{
    throw new InvalidOperationException(
        "Falta Supabase:Url o Supabase:Key. Configura Supabase__Url y Supabase__Key " +
        "con la URL del proyecto y una clave anon/publishable.");
}

builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
    {
        AutoRefreshToken = true
    }));

// ==========================================
// 3. CONFIGURACION DE CORS
// ==========================================
var myAllowSpecificOrigins = "AllowSpecificOrigins";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["https://localhost:7146", "http://localhost:5259"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// ==========================================
// 4. INYECCION DE DEPENDENCIAS (SERVICES & REPOS)
// ==========================================

// Core / Patron Unit of Work
builder.Services.AddScoped<UnitOfWork>();

// Repositorios de Infraestructura
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PropertyRepository>();
builder.Services.AddScoped<ContractRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<VisitRepository>();

// Servicios de Aplicacion (Logica de Negocio)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<VisitService>();

// ==========================================
// 5. CONTROLADORES Y DOCUMENTACION (SWAGGER)
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 6. PIPELINE DE PETICIONES HTTP (MIDDLEWARES)
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(myAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();

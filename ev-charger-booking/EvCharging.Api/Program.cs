using System.Text;
using EvCharging.Api.Middleware;
using EvCharging.Application.Contracts;
using EvCharging.Domain.Entities;
using EvCharging.Infrastructure.Config;
using EvCharging.Infrastructure.Persistence;
using EvCharging.Infrastructure.Repositories;
using EvCharging.Infrastructure.Security;
using EvCharging.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Configuration binding
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

// Repositories (generic)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>(); // ✅ NEW: bookings

builder.Services.AddControllers();

// JWT Auth
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Global exception handler
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed data + ensure indexes
await SeedAsync(app.Services);
await EnsureIndexesAsync(app.Services);

app.Run();

// ---- helpers ----

static async Task SeedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<IRepository<User>>();

    var existing = await users.FindAsync(u => u.Username == "admin");
    if (existing.Count == 0)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Username = "admin",
            PasswordHash = EvCharging.Infrastructure.Security.PasswordHasher.Hash("Admin@123"),
            Role = EvCharging.Domain.Enums.Roles.Backoffice,
            IsActive = true
        };
        await users.InsertAsync(user);
        Console.WriteLine("Seeded default admin user: admin / Admin@123");
    }
}

static async Task EnsureIndexesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await EvCharging.Infrastructure.Persistence.IndexInitializer.EnsureIndexesAsync(ctx);
}

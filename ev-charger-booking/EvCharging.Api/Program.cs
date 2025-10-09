using System.Text;
using EvCharging.Api.Middleware;
using EvCharging.Application.Contracts;
using EvCharging.Infrastructure.Config;
using EvCharging.Infrastructure.Persistence;
using EvCharging.Infrastructure.Repositories;
using EvCharging.Infrastructure.Security;
using EvCharging.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization;

// strong-typed seed usings
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

// Ignore any extra/unknown fields in Mongo documents globally
var ignoreExtras = new ConventionPack { new IgnoreExtraElementsConvention(true) };
ConventionRegistry.Register("EVCS.IgnoreExtraElements", ignoreExtras, _ => true);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();
builder.Host.UseSerilog();

// ---- Config (Mongo)
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

// ---- CORS (allow Vite dev server)
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Frontend", p =>
        p.WithOrigins("http://localhost:5173", "https://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// ---- Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ---- Services
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddControllers();

// ---- Auth (JWT)
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // dev-friendly
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

// ---- CORS early
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ In dev we do NOT force HTTPS redirection
// Only redirect in production (optional)
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---- Seed default admin user (strongly-typed; fixes CS1977)
await SeedAsync(app.Services);
// await EnsureIndexesAsync(app.Services); // Temporarily disabled due to serialization conflicts

app.Run();

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
            IsActive = true,
            Nic = "000000000V",
            FullName = "System Administrator",
            Email = "admin@evcharging.com",
            Phone = "+94701234567"
        };
        await users.InsertAsync(user);
        Console.WriteLine("Seeded default admin user: admin / Admin@123");
    }
}

// static async Task EnsureIndexesAsync(IServiceProvider services)
// {
//     using var scope = services.CreateScope();
//     var ctx = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
//     await EvCharging.Infrastructure.Persistence.IndexInitializer.EnsureIndexesAsync(ctx);
// }

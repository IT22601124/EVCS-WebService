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

// FluentValidation
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();
builder.Host.UseSerilog();

// Configuration (Mongo)
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();

// Generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();

// MVC / Controllers
builder.Services.AddControllers();

// FluentValidation: auto-run + discover validators from Application assembly
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<EvCharging.Application.Validators.CreateOwnerRequestValidator>();

// Authentication (JWT)
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

// CORS (dev-friendly; tighten in prod)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
        b.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Global exception handling
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(configure =>
{
    configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    configure.IncludeQueryInRequestPath = true;
});
// app.UseHttpsRedirection(); // Commented out for local testing with HTTP
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed admin user + ensure Mongo indexes
await SeedAsync(app.Services);
// await EnsureIndexesAsync(app.Services); // Temporarily disabled due to serialization conflicts

app.Run();

// ----------------- Helpers -----------------

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

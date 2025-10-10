using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StationService;
using StationService.Repositories;
using StationService.Services;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// 1️⃣  Configure Services
// ===================================================

// Controllers
builder.Services.AddControllers();
// Swagger (API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StationService API",
        Version = "v1",
        Description = "API for managing station data and related resources."
    });
});

// Database (EF Core)
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS (allow frontend apps)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5080") // FE hosts
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Dependency Injection (repositories & services)
builder.Services.AddScoped<IStationRepository, StationRepository>();
builder.Services.AddScoped<IStationService, StationService.Services.StationService>();


var app = builder.Build();



// ===================================================
// 3️⃣  Middleware Pipeline
// ===================================================

if (app.Environment.IsDevelopment())
{
    // Swagger only in Development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StationService API V1");
        c.RoutePrefix = string.Empty; // Swagger UI tại "/"
    });
}

// Redirect HTTP → HTTPS (vẫn cho phép chạy HTTP trong dev)
app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

// No Authentication/Authorization (public API)
app.MapControllers();

// Run the app
app.Run();

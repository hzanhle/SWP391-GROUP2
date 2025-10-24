using AdminDashboardService.Data;
using AdminDashboardService.ExternalDbContexts;
using AdminDashboardService.Repositories;
using AdminDashboardService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// AdminDashboard's own database
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AdminDashboardConnection")));

// External Service Databases (CHỈ ĐỌC LẤY DỮ LIỆU)
builder.Services.AddDbContext<UserServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserServiceConnection"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddDbContext<StationServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StationServiceConnection"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddDbContext<TwoWheelVehicleServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TwoWheelVehicleServiceConnection"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddDbContext<BookingServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookingServiceConnection"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

// ===================================================
// 1️⃣ Configure Services
// ===================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger (với cấu hình cho JWT)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AdminDashboardService API",
        Version = "v1",
        Description = "API for providing summary metrics and analytics for the admin dashboard."
    });

    // <-- THÊM DÒNG NÀY ĐỂ GIẢI QUYẾT XUNG ĐỘT TÊN MODEL
    options.CustomSchemaIds(type => type.FullName);

    // Cho phép nhập Bearer Token vào Swagger UI để test API
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Dependency Injection
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService.Services.AdminDashboardService>();

// <--- THÊM CẤU HÌNH XÁC THỰC JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be configured in appsettings.json and be at least 32 characters long");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "roleName"
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

// ===================================================
// 2️⃣ Middleware Pipeline
// ===================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdminDashboardService API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StationService;
using StationService.Repositories;
using StationService.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// 1️⃣  Configure Services
// ===================================================

// Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Cấu hình dùng để ngắt các vòng lặp tham chiếu khi serialize JSON
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddHttpClient();

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

    // Thêm nút "Authorize" vào Swagger UI để nhập token
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

// Feedback
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

//StaffShift
builder.Services.AddScoped<IStaffShiftRepository, StaffShiftRepository>();
builder.Services.AddScoped<IStaffShiftService, StaffShiftService>();

//User Intergration
builder.Services.AddScoped<IUserIntegrationService, UserIntegrationService>();

// CẤU HÌNH JWT
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

// Middleware xác thực sẽ kiểm tra token trong mỗi request
app.UseAuthentication();
// Middleware phân quyền sẽ kiểm tra vai trò (Roles) của người dùng
app.UseAuthorization();

// No Authentication/Authorization (public API)
app.MapControllers();

// Run the app
app.Run();

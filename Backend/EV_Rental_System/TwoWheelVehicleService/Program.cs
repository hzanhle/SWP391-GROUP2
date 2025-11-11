using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TwoWheelVehicleService;
using TwoWheelVehicleService.Repositories;
using TwoWheelVehicleService.Services;

var builder = WebApplication.CreateBuilder(args);

// ====================== Controllers & Swagger ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Two Wheel Vehicle API",
        Version = "v1",
        Description = "API quản lý xe hai bánh, tích hợp JWT Authentication",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@example.com"
        }
    });

    // Cấu hình JWT cho Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"Nhập token JWT của bạn (chỉ token, không cần chữ 'Bearer').  
Ví dụ:  
`eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// ====================== Database ======================
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ====================== CORS ======================
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

// ====================== Dependency Injection ======================
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IModelRepository, ModelRepository>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<ITransferVehicleRepository, TransferVehicleRepository>();
builder.Services.AddScoped<ITransferVehicleService, TransferVehicleService>();

// ====================== JWT Configuration ======================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is missing in configuration");
}
if (secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // set true khi production
    options.SaveToken = true;
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
        RequireExpirationTime = true,
        RoleClaimType = "roleName"
    };

    // Sự kiện xử lý token
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
                context.Response.Headers.Add("Access-Control-Expose-Headers", "Token-Expired");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var errorMessage = context.AuthenticateFailure?.Message ?? "Token không hợp lệ hoặc đã hết hạn";
            var response = new
            {
                success = false,
                message = errorMessage,
                statusCode = 401
            };

            return context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(response));
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst("userId")?.Value;
            logger.LogInformation("✅ Token validated successfully for user: {UserId}", userId);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ====================== Build App ======================
var app = builder.Build();

// ====================== Static Files ======================
app.UseStaticFiles();

// Đường dẫn Data/Vehicles
var vehicleDir = Path.Combine(builder.Environment.ContentRootPath, "Data", "Vehicles");
if (!Directory.Exists(vehicleDir))
{
    Directory.CreateDirectory(vehicleDir);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(vehicleDir),
    RequestPath = "/Data/Vehicles"
});

// ====================== Middleware ======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Two Wheel Vehicle API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors();

// JWT middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ====================== Log ======================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 TwoWheelVehicleService started successfully");
logger.LogInformation("🔐 JWT Authentication enabled");
logger.LogInformation("📘 Swagger UI available at: {Url}", app.Environment.IsDevelopment() ? "https://localhost:7xxx/" : "Disabled");

app.Run();

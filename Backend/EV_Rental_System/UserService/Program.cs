using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserService;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;
using UserService.Swagger;

var builder = WebApplication.CreateBuilder(args);

// ====================== Controllers & API Explorer ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ====================== Swagger Configuration with JWT ======================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Service API",
        Version = "v1",
        Description = "API for User Management with JWT Authentication",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@example.com"
        }
    });

    // Cấu hình JWT Authentication cho Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header sử dụng Bearer scheme. 
                      
        Nhập token JWT của bạn vào ô bên dưới (KHÔNG cần thêm chữ 'Bearer').

        Ví dụ: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'

        Bước thực hiện:
        1. Gọi API /api/auth/login để lấy token
        2. Copy token từ response
        3. Click nút 'Authorize' ở trên
        4. Paste token vào ô và click 'Authorize'"
    });

    // Use operation filter to only apply security to endpoints with [Authorize]
    // This respects [AllowAnonymous] attributes (e.g., login, register)
    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

// ====================== Database Configuration ======================
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    ));

// ====================== CORS Configuration ======================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ====================== Dependency Injection ======================
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICitizenInfoRepository, CitizenInfoRepository>();
builder.Services.AddScoped<IDriverLicenseRepository, DriverLicenseRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService.Services.UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICitizenInfoService, CitizenInfoService>();
builder.Services.AddScoped<IDriverLicenseService, DriverLicenseService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// Configuration Settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Cache
builder.Services.AddDistributedMemoryCache();

// ====================== JWT Authentication Configuration ======================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Validate JWT configuration
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is missing in configuration");
}

if (secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long for security");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
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
        ClockSkew = TimeSpan.Zero, // Không cho phép độ lệch thời gian

        RequireExpirationTime = true,
        RoleClaimType = "roleName"
    };

    // Event handlers for better error handling
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
            // Log successful token validation (optional)
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst("userId")?.Value;
            logger.LogInformation("Token validated successfully for user: {UserId}", userId);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ====================== Build Application ======================
var app = builder.Build();

// ====================== Middleware Pipeline ======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API V1");
        options.RoutePrefix = string.Empty; // Swagger tại root URL
        options.DocumentTitle = "User Service API Documentation";
        options.DefaultModelsExpandDepth(-1); // Ẩn schemas mặc định
        options.DisplayRequestDuration(); // Hiển thị thời gian request
        options.EnableDeepLinking(); // Enable deep linking
        options.EnableFilter(); // Enable filter
        options.EnableValidator(); // Enable validator
    });
}
else
{
    // Production: chỉ enable Swagger nếu cần
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// IMPORTANT: Authentication phải đứng trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ====================== Application Information ======================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Application started successfully");
logger.LogInformation("📝 Swagger UI available at: {Url}", app.Environment.IsDevelopment() ? "https://localhost:7xxx/" : "Disabled");
logger.LogInformation("🔐 JWT Authentication enabled");

app.Run();
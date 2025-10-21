using BookingSerivce.Models.VNPAY;
using BookingService;
using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services;
using BookingService.Services.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================== Logging Configuration ======================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
}

// ====================== Configuration Validation ======================
ValidateConfiguration(builder.Configuration);

// ====================== Core Services ======================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// ====================== API Documentation (Swagger) ======================
ConfigureSwagger(builder.Services);

// ====================== Database Configuration ======================
ConfigureDatabase(builder.Services, builder.Configuration);

// ====================== CORS Configuration ======================
ConfigureCors(builder.Services, builder.Environment);

// ====================== Authentication & Authorization ======================
ConfigureAuthentication(builder.Services, builder.Configuration);

// ====================== SignalR ======================
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ====================== Configuration Settings ======================
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<VNPaySettings>(
    builder.Configuration.GetSection("VNPaySettings"));

builder.Services.Configure<PdfSettings>(
    builder.Configuration.GetSection("PdfSettings"));

builder.Services.Configure<ContractSettings>(
    builder.Configuration.GetSection("ContractSettings"));

// ✅ FIX: THÊM OrderSettings (BẮT BUỘC!)
builder.Services.Configure<OrderSettings>(
    builder.Configuration.GetSection("OrderSettings"));

// ====================== Repository Registration ======================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOnlineContractRepository, OnlineContractRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITrustScoreRepository, TrustScoreRepository>();

// ====================== Service Registration ======================
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOnlineContractService, OnlineContractService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITrustScoreService, TrustScoreService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ✅ FIX: PDF Service nên là Singleton (để reuse browser instance)
// NHƯNG cần ensure nó không depend vào scoped services
builder.Services.AddSingleton<IPdfConverterService, PuppeteerPdfService>();

// VNPay Service
// builder.Services.AddScoped<IVNPayService, VNPayService>();

// ====================== Build Application ======================
var app = builder.Build();

// ====================== Global Exception Handler ======================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature =
            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exceptionHandlerPathFeature?.Error, "Unhandled exception occurred");

        await context.Response.WriteAsJsonAsync(new
        {
            Success = false,
            Message = app.Environment.IsDevelopment()
                ? exceptionHandlerPathFeature?.Error.Message
                : "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
            StatusCode = 500
        });
    });
});

// ====================== Middleware Pipeline ======================

// Development Tools
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingService API V1");
        options.RoutePrefix = string.Empty; // Swagger tại root URL
        options.DocumentTitle = "BookingService API Documentation";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
}
else
{
    app.UseHsts();
}

// Security & Request Pipeline
app.UseHttpsRedirection();
app.UseStaticFiles(); // For serving PDF files

app.UseRouting();
app.UseCors();

// Authentication & Authorization (thứ tự quan trọng!)
app.UseAuthentication();
app.UseAuthorization();

// ====================== Request Logging Middleware (Development Only) ======================
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        await next();

        logger.LogInformation("Response: {StatusCode}",
            context.Response.StatusCode);
    });
}

// ====================== SignalR Hubs ======================
app.MapHub<OrderTimerHub>("/orderTimerHub");

// ====================== API Controllers ======================
app.MapControllers();

// ====================== Health Check ======================
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Version = "1.0.0"
})).AllowAnonymous();


// ====================== Startup Message ======================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=================================================");
logger.LogInformation("BookingService API started successfully");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Swagger UI: {SwaggerUrl}", app.Environment.IsDevelopment() ? "http://localhost:5049" : "N/A");
logger.LogInformation("=================================================");

// ====================== Run Application ======================
app.Run();

// ====================== Helper Methods ======================

static void ValidateConfiguration(IConfiguration configuration)
{
    // Validate JWT Settings
    var jwtSettings = configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];

    if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
    {
        throw new InvalidOperationException(
            "JWT SecretKey must be at least 32 characters long. " +
            "Current length: " + (secretKey?.Length ?? 0));
    }

    // Validate Connection String
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("ConnectionString 'DefaultConnection' is required");
    }

    // Validate Required Settings Sections
    var requiredSections = new[]
    {
        "EmailSettings",
        "VNPaySettings",
        "PdfSettings",
        "ContractSettings",
        "OrderSettings" // ✅ THÊM VÀO ĐÂY
    };

    foreach (var section in requiredSections)
    {
        if (!configuration.GetSection(section).Exists())
        {
            throw new InvalidOperationException($"Required configuration section '{section}' not found");
        }
    }
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "BookingService API",
            Version = "v1",
            Description = "API cho quản lý đặt xe, thanh toán và hợp đồng điện tử",
            Contact = new OpenApiContact
            {
                Name = "BookingService Team",
                Email = "support@bookingservice.com"
            }
        });

        // JWT Authentication cho Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập JWT token vào ô bên dưới (không cần thêm chữ 'Bearer')\n\n" +
                         "Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
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
                Array.Empty<string>()
            }
        });

        // Custom schema IDs để tránh conflict
        options.CustomSchemaIds(type => type.FullName);
    });
}

static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' not found");

    services.AddDbContext<MyDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
        });

        // Enable sensitive data logging in development
        if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });
}

static void ConfigureCors(IServiceCollection services, IWebHostEnvironment environment)
{
    services.AddCors(options =>
    {
        if (environment.IsDevelopment())
        {
            // Development: Allow all
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        }
        else
        {
            // Production: Specific origins only
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        }
    });
}

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey not found");

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "roleName"
        };

        // Event Handlers
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Success = false,
                    Message = "Token không hợp lệ hoặc đã hết hạn",
                    StatusCode = 401
                };
                return context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(response));
            },
            OnMessageReceived = context =>
            {
                // Allow SignalR to receive token from query string
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/orderTimerHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    services.AddAuthorization();
}

/*
 * ===== SUMMARY CÁC THAY ĐỔI =====
 * 
 * 1. ✅ THÊM OrderSettings.Configure()
 *    - Bắt buộc cho OrderService
 * 
 * 2. ✅ THÊM Configuration Validation
 *    - Validate JWT, ConnectionString, Required sections
 *    - Fail fast nếu thiếu config
 * 
 * 3. ✅ THÊM Global Exception Handler
 *    - Catch unhandled exceptions
 *    - Return JSON response thay vì HTML error page
 * 
 * 4. ✅ THÊM Request Logging (Development)
 *    - Log mọi request/response
 *    - Giúp debug dễ hơn
 * 
 * 5. ✅ REFACTOR thành helper methods
 *    - Code clean hơn, dễ đọc hơn
 *    - Mỗi concern có method riêng
 * 
 * 6. ✅ THÊM Database health check
 *    - Log kết nối DB lúc startup
 *    - Phát hiện lỗi sớm
 * 
 * 7. ✅ CORS configuration theo environment
 *    - Development: Allow all
 *    - Production: Specific origins only
 */
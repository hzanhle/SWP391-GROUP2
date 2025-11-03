using BookingSerivce.Models.VNPAY;
using BookingService;
using BookingService.Models;
using BookingService.Models.ModelSettings;
using BookingService.Repositories;
using BookingService.Services;
using BookingService.Services.SignalR;
using BookingService.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================== Logging ======================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
}

// ====================== Validate Config ======================
ValidateConfiguration(builder.Configuration);

// ====================== Controllers & JSON ======================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// ====================== Swagger ======================
ConfigureSwagger(builder.Services);

// ====================== Database ======================
ConfigureDatabase(builder.Services, builder.Configuration);

// ====================== CORS ======================
ConfigureCors(builder.Services, builder.Environment);

// ====================== Authentication ======================
ConfigureAuthentication(builder.Services, builder.Configuration);

// ====================== SignalR ======================
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ====================== Settings ======================
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<VNPaySettings>(builder.Configuration.GetSection("VNPaySettings"));
builder.Services.Configure<PdfSettings>(builder.Configuration.GetSection("PdfSettings"));
builder.Services.Configure<ContractSettings>(builder.Configuration.GetSection("ContractSettings"));
builder.Services.Configure<OrderSettings>(builder.Configuration.GetSection("OrderSettings"));
builder.Services.Configure<AwsS3Settings>(builder.Configuration.GetSection("AwsS3Settings"));
builder.Services.Configure<BillingSettings>(builder.Configuration.GetSection("BillingSettings"));


// ====================== Repositories ======================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOnlineContractRepository, OnlineContractRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITrustScoreRepository, TrustScoreRepository>();
builder.Services.AddScoped<IFeedBackRepository, FeedBackRepository>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
builder.Services.AddScoped<ITrustScoreHistoryRepository, TrustScoreHistoryRepository>();
builder.Services.AddScoped<IVehicleCheckInRepository, VehicleCheckInRepository>();
builder.Services.AddScoped<IVehicleReturnRepository, VehicleReturnRepository>();

// ====================== Services ======================
builder.Services.AddScoped<IAwsS3Service, AwsS3Service>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOnlineContractService, OnlineContractService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITrustScoreService, TrustScoreService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddSingleton<IPdfConverterService, PuppeteerPdfService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();

// ====================== Build App ======================
var app = builder.Build();

// ====================== Global Exception Handler ======================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionFeature = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exceptionFeature?.Error, "Unhandled exception");

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = app.Environment.IsDevelopment()
                ? exceptionFeature?.Error.Message
                : "Lỗi hệ thống. Vui lòng thử lại sau.",
            statusCode = 500
        });
    });
});

// ====================== Middleware Pipeline ======================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingService API V1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "BookingService API";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ====================== Request Logging (Dev only) ======================
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("→ {Method} {Path}", context.Request.Method, context.Request.Path);
        await next();
        logger.LogInformation("← {StatusCode}", context.Response.StatusCode);
    });
}

// ====================== SignalR & Endpoints ======================
app.MapHub<OrderTimerHub>("/orderTimerHub");
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
})).AllowAnonymous();

// ====================== Startup Log ======================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=====================================");
logger.LogInformation("🚀 BookingService API Started");
logger.LogInformation("Environment: {Env}", app.Environment.EnvironmentName);
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Swagger: http://localhost:5049");
}
logger.LogInformation("=====================================");

app.Run();

// ====================== Helper Methods ======================

static void ValidateConfiguration(IConfiguration config)
{
    var errors = new List<string>();

    // JWT
    var jwtSecret = config["JwtSettings:SecretKey"];
    if (string.IsNullOrEmpty(jwtSecret))
    {
        errors.Add("JwtSettings:SecretKey không được để trống");
    }
    else if (jwtSecret.Length < 32)
    {
        errors.Add($"JWT SecretKey phải >= 32 ký tự. Hiện tại: {jwtSecret.Length}");
    }

    // Connection String
    var connStr = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connStr))
    {
        errors.Add("ConnectionString 'DefaultConnection' không tồn tại");
    }

    // Required sections
    var required = new[] {"EmailSettings", "VNPaySettings", "PdfSettings", "ContractSettings", "OrderSettings", "BillingSettings" };
    foreach (var section in required)
    {
        if (!config.GetSection(section).Exists())
        {
            errors.Add($"Section '{section}' không tồn tại");
        }
    }



    // Throw all errors at once
    if (errors.Any())
    {
        var errorMessage = "❌ Configuration validation failed:\n" +
                          string.Join("\n", errors.Select(e => $"  • {e}"));
        throw new InvalidOperationException(errorMessage);            // Dòng bị lỗi
    }

    Console.WriteLine("✓ Configuration validation passed");
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "BookingService API",
            Version = "v1",
            Description = "API quản lý đặt xe, thanh toán, hợp đồng"
        });

        // JWT Auth
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Nhập JWT token (không cần 'Bearer')\nVí dụ: eyJhbGciOiJIUzI1NiIs...",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });

        // Use operation filter to only apply security to endpoints with [Authorize]
        // This respects [AllowAnonymous] attributes (e.g., payment webhooks)
        options.OperationFilter<AuthorizeCheckOperationFilter>();

        // Use operation filter to handle file uploads with IFormFile
        options.OperationFilter<FileUploadOperationFilter>();

        options.CustomSchemaIds(type => type.FullName);
    });
}

static void ConfigureDatabase(IServiceCollection services, IConfiguration config)
{
    var connStr = config.GetConnectionString("DefaultConnection");

    services.AddDbContext<MyDbContext>(options =>
    {
        options.UseSqlServer(connStr, sqlOptions =>
        {
            // TẮT RETRY STRATEGY để tránh lỗi transaction
            sqlOptions.EnableRetryOnFailure(0);
            sqlOptions.CommandTimeout(30);
        });

        // Dev only
        if (config.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });
}

static void ConfigureCors(IServiceCollection services, IWebHostEnvironment env)
{
    services.AddCors(options =>
    {
        if (env.IsDevelopment())
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.SetIsOriginAllowed(_ => true)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        }
        else
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://yourdomain.com")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        }
    });
}

static void ConfigureAuthentication(IServiceCollection services, IConfiguration config)
{
    var jwtSettings = config.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"]!;

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                RoleClaimType = "roleName" // Phải khớp với JWT token
            };

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

                    return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "Token không hợp lệ hoặc hết hạn",
                        statusCode = 401
                    }));
                },
                OnMessageReceived = context =>
                {
                    // SignalR: Token qua query string
                    var token = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/orderTimerHub"))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

    services.AddAuthorization();
}
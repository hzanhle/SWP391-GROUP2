using BookingSerivce;
using BookingSerivce.Hubs;
using BookingSerivce.Jobs;
using BookingSerivce.Models.VNPAY;
using BookingSerivce.Repositories;
using BookingSerivce.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================== Configuration ======================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Validate JWT Secret Key
if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
}

// ====================== Services Registration ======================

// Controllers & API Documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger với JWT Authentication
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookingService API",
        Version = "v1",
        Description = "API cho quản lý đặt xe và thanh toán"
    });

    // JWT Authentication cho Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token (không cần thêm chữ 'Bearer')"
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
});

// Database Context
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS Configuration
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

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            ClockSkew = TimeSpan.Zero
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

                var response = new { message = "Token không hợp lệ hoặc đã hết hạn" };
                return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
        };
    });

builder.Services.AddAuthorization();

// VNPay Configuration
builder.Services.Configure<VNPaySettings>(
    builder.Configuration.GetSection("VNPaySettings"));

// HTTP Client Services
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Repository Registration
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISoftLockRepository, SoftLockRepository>(); // Stage 1 Enhancement

// Service Registration
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITrustScoreService, TrustScoreService>(); // Stage 1 Enhancement

// Background Jobs (Stage 1 Enhancement)
builder.Services.AddScoped<OrderExpirationJob>();
builder.Services.AddScoped<SoftLockCleanupJob>();

// Hangfire Configuration (Stage 1 Enhancement)
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

// SignalR (Stage 1 Enhancement)
builder.Services.AddSignalR();

// ====================== Build Application ======================
var app = builder.Build();

// ====================== Middleware Pipeline ======================

// Development Tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingService API V1");
        options.RoutePrefix = string.Empty; // Swagger tại root URL
    });
}

// Security & Request Pipeline
app.UseHttpsRedirection();
app.UseCors();

// Stage 1 Enhancement - Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Authentication & Authorization (thứ tự quan trọng!)
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Stage 1 Enhancement - SignalR Hub
app.MapHub<OrderHub>("/hubs/orders");

// Stage 1 Enhancement - Configure Recurring Jobs
RecurringJob.AddOrUpdate<OrderExpirationJob>(
    "process-expired-orders",
    job => job.ProcessExpiredOrdersAsync(),
    "*/30 * * * * *"); // Every 30 seconds

RecurringJob.AddOrUpdate<SoftLockCleanupJob>(
    "cleanup-expired-soft-locks",
    job => job.CleanupExpiredLocksAsync(),
    "* * * * *"); // Every minute

// ====================== Run Application ======================
app.Run();

// Hangfire Authorization Filter (allows all in development)
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // In production, add proper authorization logic here
        return true;
    }
}

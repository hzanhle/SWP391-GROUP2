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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
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

// JWT Authentication (same as UserService)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (!string.IsNullOrEmpty(secretKey) && secretKey.Length >= 32)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
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
        });
    builder.Services.AddAuthorization();
}

// VNPay Settings
builder.Services.Configure<VNPaySettings>(builder.Configuration.GetSection("VNPaySettings"));

// HttpClient for inter-service communication
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISoftLockRepository, SoftLockRepository>(); // Stage 1 Enhancement

// Services
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Stage 1 Enhancement - Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseAuthentication();
app.UseAuthorization();
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

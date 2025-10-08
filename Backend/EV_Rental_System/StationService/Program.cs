using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StationService;


var builder = WebApplication.CreateBuilder(args);

// ====================== Services ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ====================== Database ======================
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// ====================== CORS ======================
// Cho phép FE gọi API (cần chỉnh sửa domain khi deploy)
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
//builder.Services.AddScoped<IVehicleRepository, VehicleRepository>(); // Example 
//builder.Services.AddScoped<IVehicleService, VehicleService>();


var app = builder.Build();


// ====================== Middleware ======================
if (app.Environment.IsDevelopment())
{
    // Swagger chỉ bật khi dev/test, để kiểm tra API nhanh
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StationService API V1");
        c.RoutePrefix = string.Empty; // Swagger UI chạy ở root "/"
    });
}


app.UseHttpsRedirection();

// Bật CORS cho phép FE gọi API
app.UseCors();


app.MapControllers();


app.Run();

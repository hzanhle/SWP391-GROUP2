using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TwoWheelVehicleService;
using TwoWheelVehicleService.Repositories;
using TwoWheelVehicleService.Services;

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
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IModelRepository, ModelRepository>();
builder.Services.AddScoped<IModelService, ModelService>();

var app = builder.Build();

//// ====================== Static Files ======================
//app.UseStaticFiles(); // Serve wwwroot mặc định

//// Serve thư mục Data/Vehicles
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
//        Path.Combine(builder.Environment.ContentRootPath, "Data", "Vehicles")),
//    RequestPath = "/Data/Vehicles"
//});

// ====================== Middleware ======================
if (app.Environment.IsDevelopment())
{
    // Swagger chỉ bật khi dev/test, để kiểm tra API nhanh
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Two Wheel Vehicle API V1");
        c.RoutePrefix = string.Empty; // Swagger UI chạy ở root "/"
    });
}


app.UseHttpsRedirection();

// Bật CORS cho phép FE gọi API
app.UseCors();

// Không có Authentication/Authorization vì đây là module CRUD sản phẩm
app.MapControllers();


app.Run();

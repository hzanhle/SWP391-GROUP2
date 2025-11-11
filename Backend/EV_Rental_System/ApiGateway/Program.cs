using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// CORS cho FE public (sửa trong appsettings.json)
var allowedOrigins = (builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
    .Select(o => o?.Trim())
    .Where(o => !string.IsNullOrWhiteSpace(o))
    .ToArray();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("NgrokCors", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// YARP từ appsettings
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Để giữ https scheme khi đứng sau ngrok
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("NgrokCors");

// (tuỳ chọn) health
app.MapGet("/", () => Results.Ok(new { ok = true, at = "gateway" }));

app.MapReverseProxy();

// Gateway chạy 1 cổng cố định
app.Run("http://localhost:5000");

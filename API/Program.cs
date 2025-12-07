using Microsoft.EntityFrameworkCore;
using Persistence;
using Stripe;
using Domain;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// 🚀 FIXED CORS - ALL LOCALHOST PORTS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")  // ✅ ALL ports!
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 🚀 Create database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    
    // Delete old DB if exists
    if (System.IO.File.Exists("app.db"))
        System.IO.File.Delete("app.db");
    
    context.Database.EnsureCreated();
    Console.WriteLine("✅ Database & Products table created!");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🚀 CRITICAL: CORS FIRST
app.UseCors("ReactCorsPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class StripeSettings
{
    public string? SecretKey { get; set; }
    public string? PublishableKey { get; set; }
}

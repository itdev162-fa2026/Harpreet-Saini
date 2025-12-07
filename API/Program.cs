using Microsoft.EntityFrameworkCore;
using Persistence;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add controllers (enables MVC-style routing for APIs)
builder.Services.AddControllers();

// Add Swagger (for API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get connection string from appsettings.json and configure DbContext
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Add CORS for cross-origin requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  // Replace with your frontend URL if different
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS policy before other middlewares
app.UseCors("ReactCorsPolicy");

app.UseHttpsRedirection();  // Redirect HTTP to HTTPS

app.UseAuthorization();  // Add this if you're using authorization

// Map API controllers to routes
app.MapControllers();  // This enables attribute routing like [Route("api/[controller]")]

// Run the application
app.Run();  // Starts the app

// Stripe settings class
public class StripeSettings
{
    public string SecretKey { get; set; }
    public string PublishableKey { get; set; }
}

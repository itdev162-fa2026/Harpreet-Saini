using Microsoft.EntityFrameworkCore;
using Persistence;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Stripe Configuration
// ---------------------------
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// ---------------------------
// Add Services
// ---------------------------

// Add controllers
builder.Services.AddControllers();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with SQLite using absolute path from appsettings.json
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  // Replace with your frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ---------------------------
// Middleware
// ---------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactCorsPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();

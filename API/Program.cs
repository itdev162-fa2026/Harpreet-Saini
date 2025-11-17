using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Text.Json.Serialization;  // Needed for ReferenceHandler

var builder = WebApplication.CreateBuilder(args);

// Add controllers with JSON options and API behavior configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent circular reference errors (Order -> OrderItems -> Order)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic 400 Bad Request; allow manual validation to return 422
        options.SuppressModelStateInvalidFilter = true;
    });

// Add Swagger (for API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with connection string
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS for cross-origin requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  // Replace with frontend URL if different
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

app.UseHttpsRedirection();

app.UseAuthorization();

// Map API controllers to routes
app.MapControllers();

app.Run();

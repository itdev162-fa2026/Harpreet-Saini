using Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add controllers (enables MVC-style routing)
builder.Services.AddControllers(); // ✅ REQUIRED

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext with SQLite provider
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite("Data Source=weather.db")); // You can rename this to product.db if you want

var app = builder.Build();

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// ✅ This enables attribute routing like [Route("products")]
app.MapControllers(); 

app.Run();

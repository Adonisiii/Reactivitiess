using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// CORS - lejon React frontend-in të komunikojë me API-n
builder.Services.AddCors();

builder.Services.AddDbContext<appDbContext>(opt =>
{
    opt.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

var app = builder.Build();

// CORS configuration
app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("http://localhost:3000"));

app.MapControllers();

// Create scope for database migration and seeding
using var scope = app.Services.CreateScope();

var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<appDbContext>();

    // Apply migrations
    await context.Database.MigrateAsync();

    // Seed initial data
    var initializer = new DbInitializer();

    await initializer.SeedData(context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();

    logger.LogError(ex, "An error occurred during migration.");

    throw;
}

app.Run();
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.Repository;
using UserManagement.Api.Repository.Interfaces;
using UserManagement.Api.Services;
using UserManagement.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
        .LogTo(Console.WriteLine,              
               Microsoft.Extensions.Logging.LogLevel.Information)
        .EnableSensitiveDataLogging()          
        .EnableDetailedErrors();
});

// ── Redis — Caché distribuida ───
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName  = "usermgmt:";
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Aplicando migraciones...");
        db.Database.Migrate();
        logger.LogInformation("Migraciones aplicadas correctamente ✓");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error aplicando migraciones");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();


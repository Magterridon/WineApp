using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WineApp.Api.Data;
using WineApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WineService>();
builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<CellarService>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<WineAppellationImporter>();
builder.Services.AddScoped<DrinkService>();
builder.Services.AddScoped<AdminWineService>();

var app = builder.Build();

// CLI mode: run the appellation importer and exit (no HTTP server started)
if (args.Contains("--import-appellations"))
{
    using var scope = app.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<WineAppellationImporter>();
    var seedFile = Path.Combine(AppContext.BaseDirectory, "Data", "Seeds", "french-appellations.json");
    var result = await importer.ImportAsync(seedFile);
    Console.WriteLine($"Import complete: {result.Total} total — {result.Created} created, {result.Updated} updated, {result.Skipped} skipped");
    return;
}

app.UseCors();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seeding failed — ensure PostgreSQL is running and connection string is correct");
    }
}

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WineApp.Api.Data;

namespace WineApp.Tests;

public class TestWebFactory : WebApplicationFactory<Program>
{
    // Each factory instance gets its own isolated in-memory database
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly InMemoryDatabaseRoot _dbRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-super-secret-key-for-testing-only-32-chars-long!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the PostgreSQL DbContextOptions<AppDbContext> registered in Program.cs
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            // Manually create InMemory options.
            // EnableServiceProviderCaching(false) prevents EF Core from reusing the
            // DI container's internal service provider (which has Npgsql services),
            // avoiding the "multiple providers registered" conflict.
            var inMemoryOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(_dbName, _dbRoot)
                .EnableServiceProviderCaching(false)
                .Options;

            services.AddSingleton(inMemoryOptions);
        });
    }
}

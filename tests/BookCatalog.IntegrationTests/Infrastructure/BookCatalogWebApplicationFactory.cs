using BookCatalog.API;
using BookCatalog.API.Options;
using BookCatalog.API.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace BookCatalog.IntegrationTests.Infrastructure;

public class BookCatalogWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer =
         new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .Build();

    private DatabaseResetter _databaseResetter = null!;

    public async ValueTask InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        _databaseResetter = await DatabaseResetter.CreateAsync(_sqlContainer.GetConnectionString());
    }
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if(_databaseResetter is not null)
            await _databaseResetter.DisposeAsync();
        await _sqlContainer.DisposeAsync();

    }
    public Task ResetDbAsync()
    {
        return _databaseResetter.ResetAsync();
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _sqlContainer.GetConnectionString();
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptor = services
                .SingleOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if(dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            services.AddOptions<DatabaseOptions>()
                  .Configure(options =>
                  {
                      options.ConnectionString = connectionString;
                  });

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        });
    }


}

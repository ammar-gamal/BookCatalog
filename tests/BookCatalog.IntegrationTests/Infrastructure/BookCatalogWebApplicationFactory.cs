using Azure.Storage.Blobs;
using BookCatalog.API;
using BookCatalog.API.Options;
using BookCatalog.API.Persistence;
using BookCatalog.IntegrationTests.Resetters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Azurite;
using Testcontainers.MsSql;

namespace BookCatalog.IntegrationTests.Infrastructure;

public class BookCatalogWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer =
         new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .Build();

    private readonly AzuriteContainer _azuriteContainer =
        new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite")
        .Build();

    private DatabaseResetter _databaseResetter = null!;
    private BlobServiceResetter _blobResetter = null!;

    public async ValueTask InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        await _azuriteContainer.StartAsync();


        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();


        var blobServiceClient = Services.GetRequiredService<BlobServiceClient>();
        _blobResetter = new BlobServiceResetter(blobServiceClient);
        _databaseResetter = await DatabaseResetter.CreateAsync(_sqlContainer.GetConnectionString());
    }
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if(_databaseResetter is not null)
            await _databaseResetter.DisposeAsync();
        await _sqlContainer.DisposeAsync();
        await _azuriteContainer.DisposeAsync();
    }
    public Task ResetDbAsync()
    {
        return _databaseResetter.ResetAsync();
    }

    public Task ResetBlobsAsync()
    {
        return _blobResetter.ResetBlobsAsync();
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dbConstr = _sqlContainer.GetConnectionString();
        var azBlobConstr = _azuriteContainer.GetConnectionString();
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
                      options.ConnectionString = dbConstr;
                  });

            services.AddOptions<BlobStorageOptions>()
                    .Configure(options =>
                    {
                        options.Endpoint = _azuriteContainer.GetBlobEndpoint();
                    });

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(dbConstr);
            });
            services.RemoveAll<BlobServiceClient>();
            services.AddSingleton(_ => new BlobServiceClient(azBlobConstr));
        });
    }


}

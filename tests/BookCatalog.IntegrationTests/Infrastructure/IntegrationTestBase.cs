using BookCatalog.API.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BookCatalog.IntegrationTests.Infrastructure;

[Collection("Database collection")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly BookCatalogWebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected IntegrationTestBase(BookCatalogWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDbAsync();
    }

    protected async Task<T> SeedAsync<T>(Func<TestDatabaseSeeder, Task<T>> seedAction)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeder = new TestDatabaseSeeder(db);
        return await seedAction(seeder);
    }
}


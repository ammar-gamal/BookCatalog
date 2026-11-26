using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Entities.Enums;
using BookCatalog.API.Options;
using BookCatalog.API.Persistence;
using BookCatalog.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace BookCatalog.IntegrationTests.Infrastructure;

[Collection("Database collection")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly BlobServiceClient _blobClient;
    protected readonly IOptionsMonitor<BlobOptions> _blobOptions;
    protected readonly BookCatalogWebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected IntegrationTestBase(BookCatalogWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _blobClient = factory.Services.GetRequiredService<BlobServiceClient>();
        _blobOptions = factory.Services.GetRequiredService<IOptionsMonitor<BlobOptions>>();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDbAsync();
        await _factory.ResetBlobsAsync();//Can we skip this because each blob's name is GUID
    }

    protected async Task<T> SeedAsync<T>(Func<TestDatabaseSeeder, Task<T>> seedAction)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeder = new TestDatabaseSeeder(db);
        return await seedAction(seeder);
    }

    public static MultipartFormDataContent ToFormData(CreateBookRequestDto request, string fileName)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(request.AuthorId.ToString()), nameof(request.AuthorId) },
            { new StringContent(request.Title), nameof(request.Title) },
            { new StringContent(request.Isbn), nameof(request.Isbn) },
            { new StringContent(request.Price.ToString(CultureInfo.InvariantCulture)), nameof(request.Price) },
            { new StringContent(request.Genre.ToString()), nameof(request.Genre) },
            { new StringContent(request.PublicationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), nameof(request.PublicationDate) }
        };

        if(!string.IsNullOrEmpty(request.Description))
            content.Add(new StringContent(request.Description), nameof(request.Description));

        var streamContent = TestImages.LoadAsStreamContent(fileName);
        content.Add(streamContent, nameof(request.CoverPhoto), fileName);
        return content;
    }
    public static MultipartFormDataContent ToFormData(UpdateCoverPhotoRequestDto request, string fileName)
    {
        var content = new MultipartFormDataContent();
        var streamContent = TestImages.LoadAsStreamContent(fileName);
        content.Add(streamContent, nameof(request.CoverPhoto), fileName);

        return content;
    }
    protected async Task<bool> BlobExistsAsync(string blobName, BlobCategory category)
    {
        var options = _blobOptions.Get(category.ToString());
        var containerClient = _blobClient.GetBlobContainerClient(options.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        return (await blobClient.ExistsAsync()).Value;
    }
    protected async Task<string> CreateBlobAsync(string blobName, BlobCategory category)
    {
        var options = _blobOptions.Get(category.ToString());
        var containerClient = _blobClient.GetBlobContainerClient(options.ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var newName = $"{Guid.NewGuid()}{Path.GetExtension(blobName)}";
        var blobClient = containerClient.GetBlobClient(newName);
        using var stream = TestImages.Load(blobName);
        var uploadOptions = new BlobUploadOptions()
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = TestImages.GetContentType(blobName)
            }
        };
        await blobClient.UploadAsync(stream, uploadOptions);
        return newName;
    }

}


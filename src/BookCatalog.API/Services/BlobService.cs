using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BookCatalog.API.Entities.Enums;
using BookCatalog.API.Options;
using BookCatalog.API.Services.Interfaces;
using BookCatalog.API.Services.ServiceResults;
using Microsoft.Extensions.Options;

namespace BookCatalog.API.Services;

public class BlobService : IFileService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IOptionsMonitor<BlobOptions> _optionsMonitor;
    private readonly ILogger<BlobService> _logger;

    public BlobService(BlobServiceClient blobServiceClient, IOptionsMonitor<BlobOptions> optionsMonitor, ILogger<BlobService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task<bool> DeleteAsync(string fileName, BlobCategory blobCategory, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting blob {FileName} in category {Category}", fileName, blobCategory);
        var option = _optionsMonitor.Get(blobCategory.ToString());
        var containerClient = _blobServiceClient.GetBlobContainerClient(option.ContainerName);

        var result = await containerClient.DeleteBlobIfExistsAsync(fileName, cancellationToken: ct);

        if(result.Value)
            _logger.LogInformation("Deleted blob {FileName} from container {Container} ({Category})",
                fileName, option.ContainerName, blobCategory);
        else
            _logger.LogInformation("Blob {FileName} not found in container {Container} ({Category}), nothing deleted",
                fileName, option.ContainerName, blobCategory);

        return result.Value;
    }

    public async Task<UploadFileResult> UploadAsync(IFormFile file, BlobCategory blobCategory, CancellationToken ct = default)
    {
        _logger.LogInformation("Trying to upload file {FileName} with category {BlobCategory}", file.FileName, blobCategory.ToString());
        var option = _optionsMonitor.Get(blobCategory.ToString());
        var containerClient = _blobServiceClient.GetBlobContainerClient(option.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blobName = $"{Guid.NewGuid()}{extension}";
        var blobClient = containerClient.GetBlobClient(blobName);
        var uploadOptions = new BlobUploadOptions()
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.Headers.ContentType
            },
            Metadata = new Dictionary<string, string>
            {
                ["originalName"] = Uri.EscapeDataString(Path.GetFileName(file.FileName))
            },
        };

        await using var stream = file.OpenReadStream();
        var response = await blobClient.UploadAsync(stream, uploadOptions, ct);
        _logger.LogInformation("Uploaded blob {BlobName}", blobName);
        return new()
        {
            FileName = blobName,
            FileURL = blobClient.Uri.ToString()
        };
    }
    public string GetURL(string fileName, BlobCategory category)
    {
        var option = _optionsMonitor.Get(category.ToString());
        return _blobServiceClient
            .GetBlobContainerClient(option.ContainerName)
            .GetBlobClient(fileName).Uri.ToString();
    }
}

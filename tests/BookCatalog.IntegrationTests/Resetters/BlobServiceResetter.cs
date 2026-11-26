using Azure.Storage.Blobs;
namespace BookCatalog.IntegrationTests.Resetters;

public class BlobServiceResetter
{
    private readonly BlobServiceClient _client;
    public BlobServiceResetter(
        BlobServiceClient client)
    {
        _client = client;
    }
    public async Task ResetBlobsAsync()
    {
        await foreach(var container in _client.GetBlobContainersAsync())
        {
            var containerClient = _client.GetBlobContainerClient(container.Name);
            await foreach(var blob in containerClient.GetBlobsAsync())
                await containerClient.DeleteBlobIfExistsAsync(blob.Name);
        }
    }
}

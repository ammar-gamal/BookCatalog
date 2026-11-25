using BookCatalog.API.Entities.Enums;
using BookCatalog.API.Services.ServiceResults;

namespace BookCatalog.API.Services.Interfaces;

public interface IFileService
{
    Task<UploadFileResult> UploadAsync(IFormFile file, BlobCategory blobCategory, CancellationToken ct = default);
    Task<bool> DeleteAsync(string fileName, BlobCategory blobCategory, CancellationToken ct = default);
    string GetURL(string fileName, BlobCategory category);
}

namespace BookCatalog.API.Services.ServiceResults;

public record UploadFileResult
{
    public string FileName { get; init; } = null!;
    public string FileURL { get; init; } = null!;
}

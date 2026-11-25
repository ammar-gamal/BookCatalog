namespace BookCatalog.API.Options;

public class BlobOptions
{
    public const string CoverPhotoSection = "Azure:BlobService:CoverPhoto";
    public const string BookSampleSection = "Azure:BlobService:BookSample";

    public string ContainerName { get; set; } = null!;
    public double MaxSizeInBytes { get; set; }
    public string[] AllowedExtensions { get; set; } = null!;

}

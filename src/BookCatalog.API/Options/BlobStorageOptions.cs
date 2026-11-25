namespace BookCatalog.API.Options;

public class BlobStorageOptions
{
    public const string SectionName = "Azure:BlobService";
    public string Endpoint { get; set; } = string.Empty;
}


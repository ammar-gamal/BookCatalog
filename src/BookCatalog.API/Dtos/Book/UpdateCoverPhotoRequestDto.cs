using BookCatalog.API.Attributes;
using BookCatalog.API.Entities.Enums;

namespace BookCatalog.API.Dtos.Book;

public record UpdateCoverPhotoRequestDto
{
    [AllowedExtensions(BlobCategory.CoverPhoto)]
    [MaxFileSize(BlobCategory.CoverPhoto)]
    public IFormFile CoverPhoto { get; set; } = null!;
}

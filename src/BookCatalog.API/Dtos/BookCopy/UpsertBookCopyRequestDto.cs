using System.ComponentModel.DataAnnotations;

namespace BookCatalog.API.Dtos.BookCopy;

public abstract record UpsertBookCopyRequestDto
{
    [MaxLength(50)]
    public string Barcode { get; init; } = null!;

    public int BookId { get; init; }
}

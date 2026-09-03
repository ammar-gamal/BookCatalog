namespace BookCatalog.API.Dtos.BookCopy;

public record BookCopyDto
{
    public int Id { get; init; }
    public string Barcode { get; init; } = null!;
    public int BookId { get; init; }
}

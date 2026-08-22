using BookCatalog.API.Entities.Enums;

namespace BookCatalog.API.Dtos.Book;

public record BookDto
{
    public int Id { get; init; }
    public string Isbn { get; init; } = null!;
    public string Author { get; init; } = null!;
    public string Title { get; init; } = null!;
    public decimal Price { get; init; }
    public string? Description { get; init; }
    public DateOnly PublicationYear { get; init; }
    public BookGenre Genre { get; init; }
}

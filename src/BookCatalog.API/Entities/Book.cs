using BookCatalog.API.Entities.Enums;
using BookCatalog.Domain.Abstractions;

namespace BookCatalog.API.Entities;

public class Book : Entity
{
    public string Isbn { get; set; } = null!;
    public string NormalizedIsbn { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Title { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public DateOnly PublicationYear { get; set; }
    public BookGenre Genre { get; set; }
}

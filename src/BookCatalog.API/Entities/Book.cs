using BookCatalog.API.Entities.Abstractions;
using BookCatalog.API.Entities.Enums;

namespace BookCatalog.API.Entities;

public class Book : BaseEntity
{
    public string Isbn { get; set; } = null!;
    public string NormalizedIsbn { get; set; } = null!;
    public string Title { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public DateOnly PublicationDate { get; set; }
    public BookGenre Genre { get; set; }
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
    public ICollection<BookCopy> BookCopies { get; set; } = new HashSet<BookCopy>();

}

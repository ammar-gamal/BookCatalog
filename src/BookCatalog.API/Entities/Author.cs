using BookCatalog.API.Entities.Abstractions;

namespace BookCatalog.API.Entities;

public class Author : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Biography { get; set; }
    public ICollection<Book> Books { get; set; } = new HashSet<Book>();
}

using BookCatalog.API.Entities.Abstractions;

namespace BookCatalog.API.Entities;

public class BookCopy : BaseEntity
{
    public string Barcode { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = new HashSet<Loan>();

}

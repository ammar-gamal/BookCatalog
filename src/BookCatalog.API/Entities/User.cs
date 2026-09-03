using BookCatalog.API.Entities.Abstractions;

namespace BookCatalog.API.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = new HashSet<Loan>();
}

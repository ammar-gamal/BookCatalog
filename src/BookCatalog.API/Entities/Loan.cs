using BookCatalog.API.Entities.Abstractions;

namespace BookCatalog.API.Entities;

public class Loan : BaseEntity
{
    public DateTimeOffset LoanDate { get; set; }
    public DateTimeOffset? ReturnedDate { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public int UserId { get; set; }
    public int BookCopyId { get; set; }
    public User User { get; set; } = null!;
    public BookCopy BookCopy { get; set; } = null!;
}

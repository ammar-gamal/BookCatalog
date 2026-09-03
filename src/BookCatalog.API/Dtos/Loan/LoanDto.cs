namespace BookCatalog.API.Dtos.Loan;

public record LoanDto
{
    public int Id { get; init; }
    public string UserEmail { get; init; } = null!;
    public string BookTitle { get; init; } = null!;
    public DateTimeOffset LoanDate { get; init; }
    public DateTimeOffset? ReturnedDate { get; init; }
    public DateTimeOffset DueDate { get; init; }
    public int UserId { get; init; }
    public int BookCopyId { get; init; }
    public int BookId { get; init; }
}

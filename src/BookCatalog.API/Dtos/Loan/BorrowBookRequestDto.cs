namespace BookCatalog.API.Dtos.Loan;

public record BorrowBookRequestDto
{
    public int BookCopyId { get; init; }
    public int UserId { get; init; }
    public DateTimeOffset DueDate { get; set; }
}

namespace BookCatalog.API.Dtos.Loan;

public record BorrowBookResponseDto
{
    public int Id { get; set; }
    public int UserId { get; init; }
    public int BookCopyId { get; init; }
    public DateTimeOffset LoanDate { get; init; }
    public DateTimeOffset? ReturnedDate { get; init; }
    public DateTimeOffset DueDate { get; set; }
}

using BookCatalog.API.Dtos.Loan;
using BookCatalog.API.Entities;

namespace BookCatalog.API.ExtensionMethods.Mapping;

public static class LoanMappingExtensions
{
    public static Loan ToEntity(this BorrowBookRequestDto dto, DateTimeOffset loanDate)
        => new()
        {
            BookCopyId = dto.BookCopyId,
            UserId = dto.UserId,
            DueDate = dto.DueDate,
            LoanDate = loanDate
        };

    public static BorrowBookResponseDto ToBorrowBookResponseDto(this Loan loan)
       => new()
       {
           Id = loan.Id,
           BookCopyId = loan.BookCopyId,
           UserId = loan.UserId,
           DueDate = loan.DueDate,
           LoanDate = loan.LoanDate,
           ReturnedDate = loan.ReturnedDate
       };
}

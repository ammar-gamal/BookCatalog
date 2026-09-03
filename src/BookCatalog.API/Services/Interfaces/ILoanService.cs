using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Dtos.Loan;
using BookCatalog.API.Utilities.Results;

namespace BookCatalog.API.Services.Interfaces;

public interface ILoanService
{
    Task<Result<PagedList<LoanDto>>> GetAllAsync(PaginationQueryParameters parameters, CancellationToken ct = default);
    Task<Result<PagedList<LoanDto>>> GetAllForBookAsync(int bookId, PaginationQueryParameters parameters, CancellationToken ct = default);
    Task<Result<PagedList<LoanDto>>> GetAllForBookCopyAsync(int bookCopyId, PaginationQueryParameters parameters, CancellationToken ct = default);
    Task<Result<PagedList<LoanDto>>> GetAllForUserAsync(int userId, PaginationQueryParameters parameters, CancellationToken ct = default);
    Task<Result<LoanDto>> GetByIdAsync(int loanId, CancellationToken ct = default);
    Task<Result<BorrowBookResponseDto>> BorrowBookAsync(BorrowBookRequestDto request, CancellationToken ct = default);
    Task<Result> ReturnBookAsync(int loanId, CancellationToken ct = default);
}

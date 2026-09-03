using BookCatalog.API.Entities;

namespace BookCatalog.API.Repositories.Interfaces;

public interface ILoanRepository : IBaseRepository<Loan>
{

    Task<bool> BookCopyHasActiveLoanAsync(int bookCopyId, CancellationToken ct = default);
    Task<bool> BookHasActiveLoanAsync(int bookId, CancellationToken ct = default);
    IQueryable<Loan> GetAllForUser(int userId);
    IQueryable<Loan> GetAllForBookCopy(int bookCopyId);
    IQueryable<Loan> GetAllForBook(int bookId);
}

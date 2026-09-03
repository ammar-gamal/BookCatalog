using BookCatalog.API.Entities;
using BookCatalog.API.Persistence;
using BookCatalog.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BookCatalog.API.Repositories.EFCore;

public class EFCoreLoanRepository : EFCoreBaseRepository<Loan>, ILoanRepository
{
    public EFCoreLoanRepository(AppDbContext context) : base(context)
    { }
    public Task<bool> BookCopyHasActiveLoanAsync(int bookCopyId, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(e => e.BookCopyId == bookCopyId && e.ReturnedDate == null, ct);
    }

    public Task<bool> BookHasActiveLoanAsync(int bookId, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(e => e.BookCopy.BookId == bookId && e.ReturnedDate == null, ct);
    }

    public IQueryable<Loan> GetAllForBook(int bookId)
    {
        return _dbSet.Where(e => e.BookCopy.BookId == bookId);
    }

    public IQueryable<Loan> GetAllForBookCopy(int bookCopyId)
    {
        return _dbSet.Where(e => e.BookCopyId == bookCopyId);
    }

    public IQueryable<Loan> GetAllForUser(int userId)
    {
        return _dbSet.Where(e => e.UserId == userId);
    }
}

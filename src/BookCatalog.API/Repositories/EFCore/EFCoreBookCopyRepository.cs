using BookCatalog.API.Entities;
using BookCatalog.API.Persistence;
using BookCatalog.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.API.Repositories.EFCore;

public class EFCoreBookCopyRepository : EFCoreBaseRepository<BookCopy>, IBookCopyRepository
{
    public EFCoreBookCopyRepository(AppDbContext context) : base(context)
    {

    }
    public Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(e => e.Barcode == barcode, ct);
    }
}

using BookCatalog.API.Entities;
using BookCatalog.API.Persistence;
using BookCatalog.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.API.Repositories.EFCore;

public class EFCoreBookRepository : EFCoreBaseRepository<Book>, IBookRepository
{
    public EFCoreBookRepository(AppDbContext context) : base(context)
    {

    }
    public Task<bool> IsIsbnTakenAsync(string normalizedIsbn, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(e => e.NormalizedIsbn == normalizedIsbn, ct);
    }
}

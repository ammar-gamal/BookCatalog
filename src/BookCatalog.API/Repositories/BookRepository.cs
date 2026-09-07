using BookCatalog.API.Entities;
using BookCatalog.API.Persistence;
using BookCatalog.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.API.Repositories;

public class BookRepository : BaseRepository<Book>, IBookRepository
{
    public BookRepository(AppDbContext context) : base(context)
    {
    }

    public Task<bool> IsIsbnTakenAsync(string normalizedIsbn, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(e => e.NormalizedIsbn == normalizedIsbn, ct);
    }
}

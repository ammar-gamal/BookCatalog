using BookCatalog.API.Entities;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Utilities.Normalizers;

namespace BookCatalog.API.Repositories.InMemory;

public class InMemoryBookRepository : InMemoryBaseRepository<Book>, IBookRepository
{
    public Task<bool> IsIsbnTakenAsync(string normalizedIsbn, CancellationToken ct = default)
    {
        return Task.FromResult(GetAll()
                              .Any(e => e.NormalizedIsbn == normalizedIsbn));
    }

}

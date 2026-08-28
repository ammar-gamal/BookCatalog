using BookCatalog.API.Entities;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Utilities.Normalizers;

namespace BookCatalog.API.Repositories.InMemory;

public class InMemoryBookRepository : InMemoryBaseRepository<Book>, IBookRepository
{
    public Task<int?> GetBookIdByIsbnAsync(string isbn, CancellationToken ct = default)
    {
        return Task.FromResult(GetAll()
                              .Where(e => e.NormalizedIsbn == IsbnNormalizer.Normalize(isbn))
                              .Select(e => (int?)e.Id)
                              .FirstOrDefault());
    }

}

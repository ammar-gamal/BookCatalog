using BookCatalog.API.Entities;
using BookCatalog.API.Repositories.Interfaces;

namespace BookCatalog.API.Repositories.InMemory;

public class InMemoryBookRepository : InMemoryBaseRepository<Book>, IBookRepository
{
    public Task<int?> GetBookIdByIsbnAsync(string isbn, CancellationToken ct = default)
    {
        return Task.FromResult(GetAll()
                              .Where(e => e.NormalizedIsbn == isbn.ToUpper())
                              .Select(e => (int?)e.Id)
                              .FirstOrDefault());
    }

}

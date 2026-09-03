using BookCatalog.API.Entities;

namespace BookCatalog.API.Repositories.Interfaces;

public interface IBookRepository : IBaseRepository<Book>
{
    Task<bool> IsIsbnTakenAsync(string normalizedIsbn, CancellationToken ct = default);

}

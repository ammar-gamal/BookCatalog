using BookCatalog.API.Entities;

namespace BookCatalog.API.Repositories.Interfaces;

public interface IBookRepository : IBaseRepository<Book>
{
    Task<int?> GetBookIdByIsbnAsync(string isbn, CancellationToken ct = default);

}

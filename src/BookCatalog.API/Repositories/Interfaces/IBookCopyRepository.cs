using BookCatalog.API.Entities;

namespace BookCatalog.API.Repositories.Interfaces;

public interface IBookCopyRepository : IBaseRepository<BookCopy>
{
    Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct = default);
}

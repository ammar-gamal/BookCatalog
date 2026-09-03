using BookCatalog.API.Dtos.BookCopy;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Utilities.Results;

namespace BookCatalog.API.Services.Interfaces;

public interface IBookCopyService
{
    Task<Result<BookCopyDto>> CreateAsync(CreateBookCopyRequestDto request, CancellationToken ct = default);
    Task<Result<PagedList<BookCopyDto>>> GetAllAsync(PaginationQueryParameters parameters, CancellationToken ct = default);
    Task<Result<BookCopyDto>> GetByIdAsync(int id, CancellationToken ct = default);
}

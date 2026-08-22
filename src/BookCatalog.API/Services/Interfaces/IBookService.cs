using BookCatalog.API.Dtos;
using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Entities;
using BookCatalog.API.Utilities.Results;

namespace BookCatalog.API.Services.Interfaces;

public interface IBookService
{
    Task<Result<PagedList<BookDto>>> GetAllAsync(BookFilterQueryParameters paramters, CancellationToken ct = default);

    Task<Result<BookDto>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Result<BookDto>> CreateAsync(CreateBookRequestDto request, CancellationToken ct = default);

    Task<Result<BookDto>> UpdateAsync(int id, UpdateBookRequestDto request, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}

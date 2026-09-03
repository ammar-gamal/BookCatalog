using BookCatalog.API.Dtos.Author;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Utilities.Results;

namespace BookCatalog.API.Services.Interfaces;

public interface IAuthorService
{
    Task<Result<PagedList<AuthorDto>>> GetAllAsync(PaginationQueryParameters paramters, CancellationToken ct = default);
    Task<Result<AuthorDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<AuthorDto>> CreateAsync(CreateAuthorRequestDto request, CancellationToken ct = default);
}

using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Dtos.User;
using BookCatalog.API.Utilities.Results;

namespace BookCatalog.API.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserDto>> CreateAsync(CreateUserRequestDto request, CancellationToken ct = default);
    Task<Result<PagedList<UserDto>>> GetAllAsync(PaginationQueryParameters parameters, CancellationToken ct = default);
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default);
}

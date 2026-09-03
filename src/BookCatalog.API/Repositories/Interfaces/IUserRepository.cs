using BookCatalog.API.Entities;

namespace BookCatalog.API.Repositories.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}

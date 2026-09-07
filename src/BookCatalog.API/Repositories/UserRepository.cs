using BookCatalog.API.Entities;
using BookCatalog.API.Persistence;
using BookCatalog.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.API.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(u => u.Email == email, ct);
    }
}

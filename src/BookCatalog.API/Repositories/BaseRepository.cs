using BookCatalog.API.Entities.Abstractions;
using BookCatalog.API.Persistence;
using BookCatalog.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.API.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    private readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public IQueryable<TEntity> GetAll()
    {
        return _dbSet;
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync([id], ct);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(e => e.Id == id, ct);
    }
}

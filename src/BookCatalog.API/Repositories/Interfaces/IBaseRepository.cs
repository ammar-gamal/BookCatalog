using BookCatalog.API.Entities.Abstractions;

namespace BookCatalog.API.Repositories.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    IQueryable<TEntity> GetAll();
    Task SaveChangesAsync(CancellationToken ct = default);
}

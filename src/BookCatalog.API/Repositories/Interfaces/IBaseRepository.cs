using BookCatalog.Domain.Abstractions;

namespace BookCatalog.API.Repositories.Interfaces;

public interface IBaseRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    IQueryable<T> GetAll();
}

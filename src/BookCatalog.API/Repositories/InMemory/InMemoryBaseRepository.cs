using BookCatalog.API.Entities.Abstractions;
using BookCatalog.API.Repositories.Interfaces;
using System.Collections.Concurrent;

namespace BookCatalog.API.Repositories.InMemory;

public class InMemoryBaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    private readonly ConcurrentDictionary<int, TEntity> _entities = new();
    private int _id = 0;
    public virtual Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        entity.Id = Interlocked.Increment(ref _id);
        _entities[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        _entities.TryRemove(entity.Id, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        var isExists = _entities.Any(e => e.Key == id);
        return Task.FromResult(isExists);
    }

    public virtual IQueryable<TEntity> GetAll()
    {
        return _entities.Values.AsQueryable();
    }

    public virtual Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _entities.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        if(!_entities.ContainsKey(entity.Id))
            return Task.CompletedTask;

        _entities[entity.Id] = entity;
        return Task.CompletedTask;
    }
}

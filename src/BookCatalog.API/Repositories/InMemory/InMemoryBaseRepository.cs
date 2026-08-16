using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.Domain.Abstractions;
using System.Collections.Concurrent;

namespace BookCatalog.API.Repositories.InMemory;

public class InMemoryBaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : Entity
{
    private readonly ConcurrentDictionary<int, TEntity> _entities = new();
    private int _id = 0;
    public virtual Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        entity.Id = Interlocked.Increment(ref _id);
        _entities[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _entities.TryRemove(id, out _);
        return Task.CompletedTask;
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

using System.Linq.Expressions;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Infrastructure.Data.Context;
using MongoDB.Driver;

namespace BTGPactual.Infrastructure.Data.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;

    protected BaseRepository(MongoDbContext context, string collectionName)
    {
        _collection = context.GetCollection<T>(collectionName);
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        ReplaceOneResult result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        DeleteResult result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
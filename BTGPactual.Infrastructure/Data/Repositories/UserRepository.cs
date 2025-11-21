using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Infrastructure.Data.Context;
using BTGPactual.Shared.Constants;
using MongoDB.Driver;

namespace BTGPactual.Infrastructure.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MongoDbContext context)
        : base(context, MongoCollections.Users)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(x => x.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _collection.Find(x => x.Username == username).FirstOrDefaultAsync();
    }
}
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Infrastructure.Data.Context;
using BTGPactual.Shared.Constants;
using MongoDB.Driver;

namespace BTGPactual.Infrastructure.Data.Repositories;

public class FundRepository : BaseRepository<Fund>, IFundRepository
{
    public FundRepository(MongoDbContext context)
        : base(context, MongoCollections.Funds)
    {
    }

    public async Task<Fund?> GetByNameAsync(string name)
    {
        return await _collection.Find(x => x.Name == name).FirstOrDefaultAsync();
    }
}
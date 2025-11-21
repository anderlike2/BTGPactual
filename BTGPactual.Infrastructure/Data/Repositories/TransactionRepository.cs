using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Infrastructure.Data.Context;
using BTGPactual.Shared.Constants;
using MongoDB.Driver;

namespace BTGPactual.Infrastructure.Data.Repositories;

public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(MongoDbContext context)
        : base(context, MongoCollections.Transactions)
    {
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId)
    {
        return await _collection
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.TransactionDate)
            .ToListAsync();
    }

    public async Task<Transaction?> GetActiveSubscriptionAsync(string userId, string fundId)
    {
        List<Transaction> transactions = await _collection
            .Find(x => x.UserId == userId && x.FundId == fundId)
            .SortByDescending(x => x.TransactionDate)
            .ToListAsync();

        Transaction? lastTransaction = transactions.FirstOrDefault();

        if (lastTransaction != null && lastTransaction.Type == TransactionType.Subscription)
        {
            return lastTransaction;
        }

        return null;
    }
}
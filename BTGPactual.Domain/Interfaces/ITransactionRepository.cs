using BTGPactual.Domain.Entities;

namespace BTGPactual.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId);
    Task<Transaction?> GetActiveSubscriptionAsync(string userId, string fundId);
}
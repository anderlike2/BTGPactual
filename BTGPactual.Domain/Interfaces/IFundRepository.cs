using BTGPactual.Domain.Entities;

namespace BTGPactual.Domain.Interfaces;

public interface IFundRepository : IRepository<Fund>
{
    Task<Fund?> GetByNameAsync(string name);
}
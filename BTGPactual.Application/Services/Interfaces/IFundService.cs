using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;

namespace BTGPactual.Application.Services.Interfaces;

public interface IFundService
{
    Task<FundResponse> GetFundByIdAsync(string fundId);
    Task<IEnumerable<FundResponse>> GetAllFundsAsync();
    Task<FundResponse> CreateFundAsync(CreateFundRequest request);
    Task<FundResponse> UpdateFundAsync(string fundId, UpdateFundRequest request);
}
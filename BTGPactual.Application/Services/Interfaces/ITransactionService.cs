using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;

namespace BTGPactual.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> SubscribeToFundAsync(string userId, SubscribeFundRequest request);
    Task<TransactionResponse> CancelSubscriptionAsync(string userId, CancelSubscriptionRequest request);
    Task<IEnumerable<TransactionResponse>> GetClientTransactionsAsync(string userId);
    Task<IEnumerable<TransactionResponse>> GetAllTransactionsAsync();
}
using BTGPactual.Domain.Enums;

namespace BTGPactual.Application.DTOs.Responses;

public class TransactionResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FundId { get; set; } = string.Empty;
    public string FundName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? CancellationDate { get; set; }
}
namespace BTGPactual.Application.DTOs.Requests;

public class SubscribeFundRequest
{
    public string FundId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
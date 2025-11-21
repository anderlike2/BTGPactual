using BTGPactual.Domain.Enums;

namespace BTGPactual.Application.DTOs.Requests;

public class CreateFundRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal MinimumAmount { get; set; }
    public FundCategory Category { get; set; }
    public string? Description { get; set; }
}
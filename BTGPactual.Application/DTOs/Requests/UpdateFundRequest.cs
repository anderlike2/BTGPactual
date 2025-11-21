using BTGPactual.Domain.Enums;

namespace BTGPactual.Application.DTOs.Requests;

public class UpdateFundRequest
{
    public string? Name { get; set; }
    public decimal? MinimumAmount { get; set; }
    public FundCategory? Category { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
using BTGPactual.Domain.Enums;

namespace BTGPactual.Application.DTOs.Responses;

public class FundResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MinimumAmount { get; set; }
    public FundCategory Category { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
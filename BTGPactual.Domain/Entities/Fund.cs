using BTGPactual.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace BTGPactual.Domain.Entities;

public class Fund : BaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("minimumAmount")]
    public decimal MinimumAmount { get; set; }

    [BsonElement("category")]
    public FundCategory Category { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("description")]
    public string? Description { get; set; }
}
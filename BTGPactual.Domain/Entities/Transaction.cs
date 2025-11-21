using BTGPactual.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace BTGPactual.Domain.Entities;

public class Transaction : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("fundId")]
    public string FundId { get; set; } = string.Empty;

    [BsonElement("type")]
    public TransactionType Type { get; set; }

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("transactionDate")]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [BsonElement("cancellationDate")]
    public DateTime? CancellationDate { get; set; }
}
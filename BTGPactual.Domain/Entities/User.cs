using BTGPactual.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace BTGPactual.Domain.Entities;

public class User : BaseEntity
{
    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("firstName")]
    public string? FirstName { get; set; }

    [BsonElement("lastName")]
    public string? LastName { get; set; }

    [BsonElement("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [BsonElement("balance")]
    public decimal Balance { get; set; }

    [BsonElement("role")]
    public UserRole Role { get; set; }

    [BsonElement("notificationPreference")]
    public NotificationType NotificationPreference { get; set; } = NotificationType.Email;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}
using BTGPactual.Domain.Enums;

namespace BTGPactual.Application.DTOs.Requests;

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public NotificationType NotificationPreference { get; set; } = NotificationType.Email;
}
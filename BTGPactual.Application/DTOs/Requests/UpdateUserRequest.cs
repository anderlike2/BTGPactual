using BTGPactual.Domain.Enums;

namespace BTGPactual.Application.DTOs.Requests;

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public NotificationType? NotificationPreference { get; set; }
    public bool? IsActive { get; set; }
}
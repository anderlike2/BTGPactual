using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;

namespace BTGPactual.Application.Services.Interfaces;

public interface INotificationService
{
    Task NotifySubscriptionAsync(User user, Fund fund, Transaction transaction);
    Task NotifyCancellationAsync(User user, Fund fund, Transaction transaction);
}
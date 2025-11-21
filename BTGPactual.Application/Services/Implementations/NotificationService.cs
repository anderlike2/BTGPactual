using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Extensions;
using Microsoft.Extensions.Logging;

namespace BTGPactual.Application.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailService emailService,
        ISmsService smsService,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task NotifySubscriptionAsync(User user, Fund fund, Transaction transaction)
    {
        _logger.LogInformation(
            "Enviando notificación de suscripción. Usuario: {UserId}, Fondo: {FundId}, Tipo: {NotificationType}",
            user.Id, fund.Id, user.NotificationPreference);

        try
        {
            if (user.NotificationPreference == NotificationType.Email)
            {
                await SendSubscriptionEmailAsync(user, fund, transaction);
            }
            else if (user.NotificationPreference == NotificationType.SMS && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                await SendSubscriptionSmsAsync(user, fund, transaction);
            }
            else if (user.NotificationPreference == NotificationType.SMS && string.IsNullOrEmpty(user.PhoneNumber))
            {
                _logger.LogWarning(
                    "Usuario prefiere SMS pero no tiene teléfono configurado. Usuario: {UserId}. Enviando email por defecto.",
                    user.Id);
                await SendSubscriptionEmailAsync(user, fund, transaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al enviar notificación de suscripción. Usuario: {UserId}, Fondo: {FundId}",
                user.Id, fund.Id);
            throw;
        }
    }

    public async Task NotifyCancellationAsync(User user, Fund fund, Transaction transaction)
    {
        _logger.LogInformation(
            "Enviando notificación de cancelación. Usuario: {UserId}, Fondo: {FundId}, Tipo: {NotificationType}",
            user.Id, fund.Id, user.NotificationPreference);

        try
        {
            if (user.NotificationPreference == NotificationType.Email)
            {
                await SendCancellationEmailAsync(user, fund, transaction);
            }
            else if (user.NotificationPreference == NotificationType.SMS && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                await SendCancellationSmsAsync(user, fund, transaction);
            }
            else if (user.NotificationPreference == NotificationType.SMS && string.IsNullOrEmpty(user.PhoneNumber))
            {
                _logger.LogWarning(
                    "Usuario prefiere SMS pero no tiene teléfono configurado. Usuario: {UserId}. Enviando email por defecto.",
                    user.Id);
                await SendCancellationEmailAsync(user, fund, transaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al enviar notificación de cancelación. Usuario: {UserId}, Fondo: {FundId}",
                user.Id, fund.Id);
            throw;
        }
    }

    #region Private Email Methods

    private async Task SendSubscriptionEmailAsync(User user, Fund fund, Transaction transaction)
    {
        string formattedAmount = transaction.Amount.ToCurrencyString();

        string body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #0066cc;'>{AppConstants.NotificationMessages.SubscriptionEmailTitle}</h2>
                <p>{string.Format(AppConstants.NotificationMessages.SubscriptionEmailBody, fund.Name)}</p>
                <p>{string.Format(AppConstants.NotificationMessages.SubscriptionEmailAmount, formattedAmount)}</p>
                <p style='margin-top: 20px;'>{AppConstants.NotificationMessages.EmailFooter}</p>
                <hr style='margin-top: 30px; border: none; border-top: 1px solid #ccc;'/>
                <p style='font-size: 12px; color: #666;'><em>{AppConstants.NotificationMessages.EmailDisclaimer}</em></p>
            </body>
            </html>
        ";

        await _emailService.SendEmailAsync(
            user.Email,
            AppConstants.NotificationMessages.SubscriptionEmailSubject,
            body);

        _logger.LogInformation("Email de suscripción enviado a: {Email}", user.Email);
    }

    private async Task SendCancellationEmailAsync(User user, Fund fund, Transaction transaction)
    {
        string formattedAmount = transaction.Amount.ToCurrencyString();

        string body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: #0066cc;'>{AppConstants.NotificationMessages.CancellationEmailTitle}</h2>
                <p>{string.Format(AppConstants.NotificationMessages.CancellationEmailBody, fund.Name)}</p>
                <p>{string.Format(AppConstants.NotificationMessages.CancellationEmailAmount, formattedAmount)}</p>
                <p style='margin-top: 20px;'>{AppConstants.NotificationMessages.EmailFooter}</p>
                <hr style='margin-top: 30px; border: none; border-top: 1px solid #ccc;'/>
                <p style='font-size: 12px; color: #666;'><em>{AppConstants.NotificationMessages.EmailDisclaimer}</em></p>
            </body>
            </html>
        ";

        await _emailService.SendEmailAsync(
            user.Email,
            AppConstants.NotificationMessages.CancellationEmailSubject,
            body);

        _logger.LogInformation("Email de cancelación enviado a: {Email}", user.Email);
    }

    #endregion

    #region Private SMS Methods

    private async Task SendSubscriptionSmsAsync(User user, Fund fund, Transaction transaction)
    {
        string formattedAmount = transaction.Amount.ToCurrencyString();

        string message = string.Format(
            AppConstants.NotificationMessages.SubscriptionSmsTemplate,
            fund.Name,
            formattedAmount);

        await _smsService.SendSmsAsync(user.PhoneNumber!, message);

        _logger.LogInformation("SMS de suscripción enviado a: {PhoneNumber}", user.PhoneNumber);
    }

    private async Task SendCancellationSmsAsync(User user, Fund fund, Transaction transaction)
    {
        string formattedAmount = transaction.Amount.ToCurrencyString();

        string message = string.Format(
            AppConstants.NotificationMessages.CancellationSmsTemplate,
            fund.Name,
            formattedAmount);

        await _smsService.SendSmsAsync(user.PhoneNumber!, message);

        _logger.LogInformation("SMS de cancelación enviado a: {PhoneNumber}", user.PhoneNumber);
    }

    #endregion
}
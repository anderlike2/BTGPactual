using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BTGPactual.Infrastructure.ExternalServices.SMS;

public class AwsSnsService : ISmsService
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly AwsSettings _awsSettings;
    private readonly ILogger<AwsSnsService> _logger;

    public AwsSnsService(
        IAmazonSimpleNotificationService snsClient,
        IOptions<AwsSettings> awsSettings,
        ILogger<AwsSnsService> logger)
    {
        _snsClient = snsClient;
        _awsSettings = awsSettings.Value;
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            PublishRequest request = new PublishRequest
            {
                PhoneNumber = phoneNumber,
                Message = message
            };

            PublishResponse response = await _snsClient.PublishAsync(request);

            _logger.LogInformation(
                "SMS enviado exitosamente. Destinatario: {PhoneNumber}, MessageId: {MessageId}",
                phoneNumber, response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar SMS a {PhoneNumber}", phoneNumber);
            throw;
        }
    }
}
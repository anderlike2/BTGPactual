using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BTGPactual.Infrastructure.ExternalServices.Email;

public class AwsSesEmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly AwsSettings _awsSettings;
    private readonly ILogger<AwsSesEmailService> _logger;

    public AwsSesEmailService(
        IAmazonSimpleEmailService sesClient,
        IOptions<AwsSettings> awsSettings,
        ILogger<AwsSesEmailService> logger)
    {
        _sesClient = sesClient;
        _awsSettings = awsSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            SendEmailRequest sendRequest = new SendEmailRequest
            {
                Source = $"{_awsSettings.Ses.FromName} <{_awsSettings.Ses.FromEmail}>",
                Destination = new Destination
                {
                    ToAddresses = new List<string> { to }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content(body)
                    }
                }
            };

            SendEmailResponse response = await _sesClient.SendEmailAsync(sendRequest);

            _logger.LogInformation(
                "Email enviado exitosamente. Destinatario: {To}, MessageId: {MessageId}",
                to, response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {To}", to);
            throw;
        }
    }
}
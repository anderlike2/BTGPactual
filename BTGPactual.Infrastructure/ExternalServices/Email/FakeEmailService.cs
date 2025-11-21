using BTGPactual.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BTGPactual.Infrastructure.ExternalServices.Email;

public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;

    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("[FAKE EMAIL] To: {To}, Subject: {Subject}", to, subject);
        _logger.LogInformation("[FAKE EMAIL] Body: {Body}", body);
        return Task.CompletedTask;
    }
}
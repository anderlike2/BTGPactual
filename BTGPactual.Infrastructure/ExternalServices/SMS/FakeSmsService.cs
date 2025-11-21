using BTGPactual.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BTGPactual.Infrastructure.ExternalServices.SMS;

public class FakeSmsService : ISmsService
{
    private readonly ILogger<FakeSmsService> _logger;

    public FakeSmsService(ILogger<FakeSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("[FAKE SMS] To: {PhoneNumber}, Message: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
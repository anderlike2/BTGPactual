namespace BTGPactual.Application.Services.Interfaces;

public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
}
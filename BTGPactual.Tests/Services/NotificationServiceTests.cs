using BTGPactual.Application.Services.Implementations;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BTGPactual.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly Mock<ILogger<NotificationService>> _loggerMock;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _smsServiceMock = new Mock<ISmsService>();
        _loggerMock = new Mock<ILogger<NotificationService>>();

        _notificationService = new NotificationService(
            _emailServiceMock.Object,
            _smsServiceMock.Object,
            _loggerMock.Object
        );
    }

    #region NotifySubscriptionAsync Tests

    [Fact]
    public async Task NotifySubscriptionAsync_WithEmailPreference_ShouldSendEmail()
    {
        var user = new User
        {
            Id = "507f1f77bcf86cd799439011",
            Email = "test@example.com",
            NotificationPreference = NotificationType.Email
        };

        var fund = new Fund
        {
            Id = "507f1f77bcf86cd799439012",
            Name = "Test Fund"
        };

        var transaction = new Transaction
        {
            Id = "507f1f77bcf86cd799439013",
            Amount = 100000m
        };

        await _notificationService.NotifySubscriptionAsync(user, fund, transaction);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            user.Email,
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);

        _smsServiceMock.Verify(x => x.SendSmsAsync(
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task NotifySubscriptionAsync_WithSmsPreferenceAndPhone_ShouldSendSms()
    {
        var user = new User
        {
            Id = "507f1f77bcf86cd799439011",
            Email = "test@example.com",
            PhoneNumber = "+573001234567",
            NotificationPreference = NotificationType.SMS
        };

        var fund = new Fund
        {
            Id = "507f1f77bcf86cd799439012",
            Name = "Test Fund"
        };

        var transaction = new Transaction
        {
            Id = "507f1f77bcf86cd799439013",
            Amount = 100000m
        };

        await _notificationService.NotifySubscriptionAsync(user, fund, transaction);

        _smsServiceMock.Verify(x => x.SendSmsAsync(
            user.PhoneNumber,
            It.IsAny<string>()
        ), Times.Once);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task NotifySubscriptionAsync_WithSmsPreferenceButNoPhone_ShouldFallbackToEmail()
    {
        var user = new User
        {
            Id = "507f1f77bcf86cd799439011",
            Email = "test@example.com",
            NotificationPreference = NotificationType.SMS
        };

        var fund = new Fund
        {
            Id = "507f1f77bcf86cd799439012",
            Name = "Test Fund"
        };

        var transaction = new Transaction
        {
            Id = "507f1f77bcf86cd799439013",
            Amount = 100000m
        };

        await _notificationService.NotifySubscriptionAsync(user, fund, transaction);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            user.Email,
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);

        _smsServiceMock.Verify(x => x.SendSmsAsync(
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    #endregion

    #region NotifyCancellationAsync Tests

    [Fact]
    public async Task NotifyCancellationAsync_WithEmailPreference_ShouldSendEmail()
    {
        var user = new User
        {
            Id = "507f1f77bcf86cd799439011",
            Email = "test@example.com",
            NotificationPreference = NotificationType.Email
        };

        var fund = new Fund
        {
            Id = "507f1f77bcf86cd799439012",
            Name = "Test Fund"
        };

        var transaction = new Transaction
        {
            Id = "507f1f77bcf86cd799439013",
            Amount = 100000m
        };

        await _notificationService.NotifyCancellationAsync(user, fund, transaction);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            user.Email,
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);

        _smsServiceMock.Verify(x => x.SendSmsAsync(
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task NotifyCancellationAsync_WithSmsPreferenceAndPhone_ShouldSendSms()
    {
        var user = new User
        {
            Id = "507f1f77bcf86cd799439011",
            Email = "test@example.com",
            PhoneNumber = "+573001234567",
            NotificationPreference = NotificationType.SMS
        };

        var fund = new Fund
        {
            Id = "507f1f77bcf86cd799439012",
            Name = "Test Fund"
        };

        var transaction = new Transaction
        {
            Id = "507f1f77bcf86cd799439013",
            Amount = 100000m
        };

        await _notificationService.NotifyCancellationAsync(user, fund, transaction);

        _smsServiceMock.Verify(x => x.SendSmsAsync(
            user.PhoneNumber,
            It.IsAny<string>()
        ), Times.Once);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task NotifyCancellationAsync_WithSmsPreferenceButNoPhone_ShouldFallbackToEmail()
    {
        var user = new User
        {
            Id = "507f1f77bcf86cd799439011",
            Email = "test@example.com",
            PhoneNumber = null,
            NotificationPreference = NotificationType.SMS
        };

        var fund = new Fund
        {
            Id = "507f1f77bcf86cd799439012",
            Name = "Test Fund"
        };

        var transaction = new Transaction
        {
            Id = "507f1f77bcf86cd799439013",
            Amount = 100000m
        };

        await _notificationService.NotifyCancellationAsync(user, fund, transaction);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            user.Email,
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);

        _smsServiceMock.Verify(x => x.SendSmsAsync(
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    #endregion
}
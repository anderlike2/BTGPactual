using AutoMapper;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Implementations;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Shared.Constants;
using BTGPactual.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BTGPactual.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFundRepository> _fundRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<TransactionService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _fundRepositoryMock = new Mock<IFundRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<TransactionService>>();
        _mapper = AutoMapperFactory.Create();

        _transactionService = new TransactionService(
            _transactionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _fundRepositoryMock.Object,
            _notificationServiceMock.Object,
            _mapper,
            _loggerMock.Object
        );
    }

    #region SubscribeToFundAsync Tests

    [Fact]
    public async Task SubscribeToFundAsync_WithValidRequest_ShouldCreateSubscription()
    {
        var userId = "507f1f77bcf86cd799439011";
        var fundId = "507f1f77bcf86cd799439012";

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Balance = 500000m,
            IsActive = true,
            NotificationPreference = NotificationType.Email
        };

        var fund = new Fund
        {
            Id = fundId,
            Name = "Test Fund",
            MinimumAmount = 100000m,
            IsActive = true
        };

        var request = new SubscribeFundRequest
        {
            FundId = fundId,
            Amount = 150000m
        };

        var createdTransaction = new Transaction
        {
            Id = "507f1f77bcf86cd799439013",
            UserId = userId,
            FundId = fundId,
            Type = TransactionType.Subscription,
            Amount = request.Amount,
            TransactionDate = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        _transactionRepositoryMock
            .Setup(x => x.GetActiveSubscriptionAsync(userId, fundId))
            .ReturnsAsync((Transaction?)null);

        _transactionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(createdTransaction);

        var result = await _transactionService.SubscribeToFundAsync(userId, request);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.FundId.Should().Be(fundId);
        result.Amount.Should().Be(request.Amount);
        result.Type.Should().Be(TransactionType.Subscription);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
            u.Balance == 350000m
        )), Times.Once);

        _transactionRepositoryMock.Verify(x => x.CreateAsync(It.Is<Transaction>(t =>
            t.UserId == userId &&
            t.FundId == fundId &&
            t.Amount == request.Amount &&
            t.Type == TransactionType.Subscription
        )), Times.Once);
    }

    [Fact]
    public async Task SubscribeToFundAsync_WithInsufficientBalance_ShouldThrowBusinessException()
    {
        var userId = "507f1f77bcf86cd799439011";
        var fundId = "507f1f77bcf86cd799439012";

        var user = new User
        {
            Id = userId,
            Balance = 50000m,
            IsActive = true
        };

        var fund = new Fund
        {
            Id = fundId,
            MinimumAmount = 100000m,
            IsActive = true
        };

        var request = new SubscribeFundRequest
        {
            FundId = fundId,
            Amount = 100000m
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        _transactionRepositoryMock
            .Setup(x => x.GetActiveSubscriptionAsync(userId, fundId))
            .ReturnsAsync((Transaction?)null);

        Func<Task> act = async () => await _transactionService.SubscribeToFundAsync(userId, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Balance insuficiente*");

        _transactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task SubscribeToFundAsync_WithAmountLessThanMinimum_ShouldThrowBusinessException()
    {
        var userId = "507f1f77bcf86cd799439011";
        var fundId = "507f1f77bcf86cd799439012";

        var user = new User
        {
            Id = userId,
            Balance = 500000m,
            IsActive = true
        };

        var fund = new Fund
        {
            Id = fundId,
            MinimumAmount = 100000m,
            IsActive = true
        };

        var request = new SubscribeFundRequest
        {
            FundId = fundId,
            Amount = 50000m
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        Func<Task> act = async () => await _transactionService.SubscribeToFundAsync(userId, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.InsufficientAmount);

        _transactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task SubscribeToFundAsync_WithInactiveFund_ShouldThrowBusinessException()
    {
        var userId = "507f1f77bcf86cd799439011";
        var fundId = "507f1f77bcf86cd799439012";

        var user = new User
        {
            Id = userId,
            Balance = 500000m,
            IsActive = true
        };

        var fund = new Fund
        {
            Id = fundId,
            MinimumAmount = 100000m,
            IsActive = false
        };

        var request = new SubscribeFundRequest
        {
            FundId = fundId,
            Amount = 100000m
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        Func<Task> act = async () => await _transactionService.SubscribeToFundAsync(userId, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.FundNotActive);

        _transactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task SubscribeToFundAsync_WhenAlreadySubscribed_ShouldThrowBusinessException()
    {
        var userId = "507f1f77bcf86cd799439011";
        var fundId = "507f1f77bcf86cd799439012";

        var user = new User
        {
            Id = userId,
            Balance = 500000m,
            IsActive = true
        };

        var fund = new Fund
        {
            Id = fundId,
            MinimumAmount = 100000m,
            IsActive = true
        };

        var request = new SubscribeFundRequest
        {
            FundId = fundId,
            Amount = 100000m
        };

        var existingSubscription = new Transaction
        {
            UserId = userId,
            FundId = fundId,
            Type = TransactionType.Subscription
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        _transactionRepositoryMock
            .Setup(x => x.GetActiveSubscriptionAsync(userId, fundId))
            .ReturnsAsync(existingSubscription);

        Func<Task> act = async () => await _transactionService.SubscribeToFundAsync(userId, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.AlreadySubscribed);

        _transactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    #endregion

    #region CancelSubscriptionAsync Tests

    [Fact]
    public async Task CancelSubscriptionAsync_WithValidRequest_ShouldCancelSubscription()
    {
        var userId = "507f1f77bcf86cd799439011";
        var fundId = "507f1f77bcf86cd799439012";

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Balance = 350000m,
            IsActive = true
        };

        var fund = new Fund
        {
            Id = fundId,
            Name = "Test Fund",
            IsActive = true
        };

        var existingSubscription = new Transaction
        {
            UserId = userId,
            FundId = fundId,
            Type = TransactionType.Subscription,
            Amount = 150000m
        };

        var request = new CancelSubscriptionRequest
        {
            FundId = fundId
        };

        var createdTransaction = new Transaction
        {
            Id = "507f1f77bcf86cd799439013",
            UserId = userId,
            FundId = fundId,
            Type = TransactionType.Cancellation,
            Amount = existingSubscription.Amount,
            TransactionDate = DateTime.UtcNow,
            CancellationDate = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        _transactionRepositoryMock
            .Setup(x => x.GetActiveSubscriptionAsync(userId, fundId))
            .ReturnsAsync(existingSubscription);

        _transactionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(createdTransaction);

        var result = await _transactionService.CancelSubscriptionAsync(userId, request);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.FundId.Should().Be(fundId);
        result.Type.Should().Be(TransactionType.Cancellation);
        result.Amount.Should().Be(existingSubscription.Amount);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
            u.Balance == 500000m
        )), Times.Once);

        _transactionRepositoryMock.Verify(x => x.CreateAsync(It.Is<Transaction>(t =>
            t.UserId == userId &&
            t.FundId == fundId &&
            t.Type == TransactionType.Cancellation &&
            t.Amount == existingSubscription.Amount
        )), Times.Once);
    }

    [Fact]
    public async Task CancelSubscriptionAsync_WhenNotSubscribed_ShouldThrowBusinessException()
    {
        var userId = "507f1f77bcf86cd799439011";
        var fundId = "507f1f77bcf86cd799439012";

        var user = new User
        {
            Id = userId,
            IsActive = true
        };

        var fund = new Fund
        {
            Id = fundId,
            IsActive = true
        };

        var request = new CancelSubscriptionRequest
        {
            FundId = fundId
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        _transactionRepositoryMock
            .Setup(x => x.GetActiveSubscriptionAsync(userId, fundId))
            .ReturnsAsync((Transaction?)null);

        Func<Task> act = async () => await _transactionService.CancelSubscriptionAsync(userId, request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.NotSubscribed);

        _transactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    #endregion

    #region GetClientTransactionsAsync Tests

    [Fact]
    public async Task GetClientTransactionsAsync_WithValidUserId_ShouldReturnTransactions()
    {
        var userId = "507f1f77bcf86cd799439011";

        var user = new User { Id = userId };

        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = "507f1f77bcf86cd799439012",
                UserId = userId,
                FundId = "507f1f77bcf86cd799439013",
                Type = TransactionType.Subscription,
                Amount = 100000m
            },
            new Transaction
            {
                Id = "507f1f77bcf86cd799439014",
                UserId = userId,
                FundId = "507f1f77bcf86cd799439013",
                Type = TransactionType.Cancellation,
                Amount = 100000m
            }
        };

        var fund = new Fund
        {
            Id = "507f1f77bcf86cd799439013",
            Name = "Test Fund"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _transactionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(transactions);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(fund);

        var result = await _transactionService.GetClientTransactionsAsync(userId);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Type == TransactionType.Subscription);
        result.Should().Contain(t => t.Type == TransactionType.Cancellation);
    }

    [Fact]
    public async Task GetClientTransactionsAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        var userId = "507f1f77bcf86cd799439011";

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _transactionService.GetClientTransactionsAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.UserNotFound);
    }

    #endregion

    #region GetAllTransactionsAsync Tests

    [Fact]
    public async Task GetAllTransactionsAsync_ShouldReturnAllTransactions()
    {
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = "507f1f77bcf86cd799439011",
                UserId = "507f1f77bcf86cd799439012",
                FundId = "507f1f77bcf86cd799439013",
                Type = TransactionType.Subscription,
                Amount = 100000m
            },
            new Transaction
            {
                Id = "507f1f77bcf86cd799439014",
                UserId = "507f1f77bcf86cd799439015",
                FundId = "507f1f77bcf86cd799439016",
                Type = TransactionType.Subscription,
                Amount = 200000m
            }
        };

        var fund = new Fund
        {
            Id = "507f1f77bcf86cd799439013",
            Name = "Test Fund"
        };

        _transactionRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(transactions);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(fund);

        var result = await _transactionService.GetAllTransactionsAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion
}
using System.Security.Claims;
using BTGPactual.API.Controllers;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BTGPactual.Tests.Controllers;

public class TransactionsControllerTests
{
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly TransactionsController _controller;
    private readonly string _testUserId = "507f1f77bcf86cd799439011";

    public TransactionsControllerTests()
    {
        _transactionServiceMock = new Mock<ITransactionService>();
        _controller = new TransactionsController(_transactionServiceMock.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
            new Claim(ClaimTypes.Role, UserRole.Client.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region Subscribe Tests

    [Fact]
    public async Task Subscribe_WithValidRequest_ShouldReturnOkWithTransaction()
    {
        var request = new SubscribeFundRequest
        {
            FundId = "507f1f77bcf86cd799439012",
            Amount = 100000m
        };

        var transactionResponse = new TransactionResponse
        {
            Id = "507f1f77bcf86cd799439013",
            UserId = _testUserId,
            FundId = request.FundId,
            FundName = "Test Fund",
            Type = TransactionType.Subscription,
            Amount = request.Amount,
            TransactionDate = DateTime.UtcNow
        };

        _transactionServiceMock
            .Setup(x => x.SubscribeToFundAsync(_testUserId, request))
            .ReturnsAsync(transactionResponse);

        var result = await _controller.SubscribeToFund(request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);

        var response = objectResult.Value.Should().BeOfType<ApiResponse<TransactionResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be(AppConstants.SuccessMessages.SubscriptionSuccessful);
        response.Data.Should().NotBeNull();
        response.Data.Type.Should().Be(TransactionType.Subscription);

        _transactionServiceMock.Verify(x => x.SubscribeToFundAsync(_testUserId, request), Times.Once);
    }

    [Fact]
    public async Task Subscribe_WithInsufficientBalance_ShouldThrowBusinessException()
    {
        var request = new SubscribeFundRequest
        {
            FundId = "507f1f77bcf86cd799439012",
            Amount = 1000000m
        };

        _transactionServiceMock
            .Setup(x => x.SubscribeToFundAsync(_testUserId, request))
            .ThrowsAsync(new BusinessException("Balance insuficiente"));

        Func<Task> act = async () => await _controller.SubscribeToFund(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Balance insuficiente*");

        _transactionServiceMock.Verify(x => x.SubscribeToFundAsync(_testUserId, request), Times.Once);
    }

    [Fact]
    public async Task Subscribe_WhenAlreadySubscribed_ShouldThrowBusinessException()
    {
        var request = new SubscribeFundRequest
        {
            FundId = "507f1f77bcf86cd799439012",
            Amount = 100000m
        };

        _transactionServiceMock
            .Setup(x => x.SubscribeToFundAsync(_testUserId, request))
            .ThrowsAsync(new BusinessException(AppConstants.ErrorMessages.AlreadySubscribed));

        Func<Task> act = async () => await _controller.SubscribeToFund(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.AlreadySubscribed);
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public async Task Cancel_WithValidRequest_ShouldReturnOkWithTransaction()
    {
        var request = new CancelSubscriptionRequest
        {
            FundId = "507f1f77bcf86cd799439012"
        };

        var transactionResponse = new TransactionResponse
        {
            Id = "507f1f77bcf86cd799439013",
            UserId = _testUserId,
            FundId = request.FundId,
            FundName = "Test Fund",
            Type = TransactionType.Cancellation,
            Amount = 100000m,
            TransactionDate = DateTime.UtcNow,
            CancellationDate = DateTime.UtcNow
        };

        _transactionServiceMock
            .Setup(x => x.CancelSubscriptionAsync(_testUserId, request))
            .ReturnsAsync(transactionResponse);

        var result = await _controller.CancelSubscription(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<TransactionResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be(AppConstants.SuccessMessages.CancellationSuccessful);
        response.Data.Should().NotBeNull();
        response.Data.Type.Should().Be(TransactionType.Cancellation);

        _transactionServiceMock.Verify(x => x.CancelSubscriptionAsync(_testUserId, request), Times.Once);
    }

    [Fact]
    public async Task Cancel_WhenNotSubscribed_ShouldThrowBusinessException()
    {
        var request = new CancelSubscriptionRequest
        {
            FundId = "507f1f77bcf86cd799439012"
        };

        _transactionServiceMock
            .Setup(x => x.CancelSubscriptionAsync(_testUserId, request))
            .ThrowsAsync(new BusinessException(AppConstants.ErrorMessages.NotSubscribed));

        Func<Task> act = async () => await _controller.CancelSubscription(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.NotSubscribed);

        _transactionServiceMock.Verify(x => x.CancelSubscriptionAsync(_testUserId, request), Times.Once);
    }

    #endregion

    #region GetMyTransactions Tests

    [Fact]
    public async Task GetMyTransactions_ShouldReturnOkWithTransactionsList()
    {
        var transactions = new List<TransactionResponse>
        {
            new TransactionResponse
            {
                Id = "507f1f77bcf86cd799439013",
                UserId = _testUserId,
                FundId = "507f1f77bcf86cd799439012",
                FundName = "Fund 1",
                Type = TransactionType.Subscription,
                Amount = 100000m
            },
            new TransactionResponse
            {
                Id = "507f1f77bcf86cd799439014",
                UserId = _testUserId,
                FundId = "507f1f77bcf86cd799439012",
                FundName = "Fund 1",
                Type = TransactionType.Cancellation,
                Amount = 100000m
            }
        };

        _transactionServiceMock
            .Setup(x => x.GetClientTransactionsAsync(_testUserId))
            .ReturnsAsync(transactions);

        var result = await _controller.GetMyTransactions();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<TransactionResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        _transactionServiceMock.Verify(x => x.GetClientTransactionsAsync(_testUserId), Times.Once);
    }

    [Fact]
    public async Task GetMyTransactions_WithNoTransactions_ShouldReturnEmptyList()
    {
        _transactionServiceMock
            .Setup(x => x.GetClientTransactionsAsync(_testUserId))
            .ReturnsAsync(new List<TransactionResponse>());

        var result = await _controller.GetMyTransactions();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<TransactionResponse>>>().Subject;
        response.Data.Should().BeEmpty();
    }

    #endregion
}
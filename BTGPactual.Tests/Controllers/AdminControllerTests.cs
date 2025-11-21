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

public class AdminControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly AdminController _controller;
    private readonly string _adminUserId = "507f1f77bcf86cd799439011";

    public AdminControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _transactionServiceMock = new Mock<ITransactionService>();

        _controller = new AdminController(
            _userServiceMock.Object,
            _transactionServiceMock.Object
        );

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _adminUserId),
            new Claim(ClaimTypes.Role, UserRole.Admin.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetAllTransactions Tests

    [Fact]
    public async Task GetAllTransactions_ShouldReturnOkWithTransactionsList()
    {
        var transactions = new List<TransactionResponse>
        {
            new TransactionResponse
            {
                Id = "507f1f77bcf86cd799439013",
                UserId = "507f1f77bcf86cd799439012",
                FundId = "507f1f77bcf86cd799439014",
                FundName = "Fund 1",
                Type = TransactionType.Subscription,
                Amount = 100000m
            },
            new TransactionResponse
            {
                Id = "507f1f77bcf86cd799439015",
                UserId = "507f1f77bcf86cd799439016",
                FundId = "507f1f77bcf86cd799439017",
                FundName = "Fund 2",
                Type = TransactionType.Subscription,
                Amount = 200000m
            }
        };

        _transactionServiceMock
            .Setup(x => x.GetAllTransactionsAsync())
            .ReturnsAsync(transactions);

        var result = await _controller.GetAllTransactions();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<TransactionResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        _transactionServiceMock.Verify(x => x.GetAllTransactionsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllTransactions_WithNoTransactions_ShouldReturnEmptyList()
    {
        _transactionServiceMock
            .Setup(x => x.GetAllTransactionsAsync())
            .ReturnsAsync(new List<TransactionResponse>());

        var result = await _controller.GetAllTransactions();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<TransactionResponse>>>().Subject;
        response.Data.Should().BeEmpty();
    }

    #endregion

    #region GetAllUsers Tests

    [Fact]
    public async Task GetAllUsers_ShouldReturnOkWithUsersList()
    {
        var users = new List<UserResponse>
        {
            new UserResponse
            {
                Id = "507f1f77bcf86cd799439012",
                Username = "user1",
                Email = "user1@example.com",
                Role = UserRole.Client,
                Balance = 500000m,
                IsActive = true
            },
            new UserResponse
            {
                Id = "507f1f77bcf86cd799439013",
                Username = "user2",
                Email = "user2@example.com",
                Role = UserRole.Client,
                Balance = 300000m,
                IsActive = true
            }
        };

        _userServiceMock
            .Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(users);

        var result = await _controller.GetAllUsers();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<UserResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        _userServiceMock.Verify(x => x.GetAllUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_WithNoUsers_ShouldReturnEmptyList()
    {
        _userServiceMock
            .Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(new List<UserResponse>());

        var result = await _controller.GetAllUsers();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<UserResponse>>>().Subject;
        response.Data.Should().BeEmpty();
    }

    #endregion

    #region GetUserById Tests

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturnOkWithUser()
    {
        var userId = "507f1f77bcf86cd799439012";
        var user = new UserResponse
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.Client,
            Balance = 500000m,
            IsActive = true
        };

        _userServiceMock
            .Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        var result = await _controller.GetUserById(userId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<UserResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(userId);

        _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ShouldReturnBadRequest()
    {
        var invalidId = "invalid-id";

        var result = await _controller.GetUserById(invalidId);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be(AppConstants.ValidationMessages.InvalidObjectId);

        _userServiceMock.Verify(x => x.GetUserByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserById_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var userId = "507f1f77bcf86cd799439012";

        _userServiceMock
            .Setup(x => x.GetUserByIdAsync(userId))
            .ThrowsAsync(new NotFoundException(AppConstants.ErrorMessages.UserNotFound));

        Func<Task> act = async () => await _controller.GetUserById(userId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.UserNotFound);

        _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    #endregion

    #region UpdateUser Tests

    [Fact]
    public async Task UpdateUser_WithValidRequest_ShouldReturnOkWithUpdatedUser()
    {
        var userId = "507f1f77bcf86cd799439012";
        var request = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            IsActive = false
        };

        var updatedUser = new UserResponse
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Updated",
            LastName = "Name",
            Role = UserRole.Client,
            IsActive = false
        };

        _userServiceMock
            .Setup(x => x.UpdateUserAsync(userId, request))
            .ReturnsAsync(updatedUser);

        var result = await _controller.UpdateUser(userId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<UserResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be(AppConstants.SuccessMessages.OperationSuccessful);
        response.Data.Should().NotBeNull();
        response.Data.FirstName.Should().Be("Updated");
        response.Data.IsActive.Should().BeFalse();

        _userServiceMock.Verify(x => x.UpdateUserAsync(userId, request), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidId_ShouldReturnBadRequest()
    {
        var invalidId = "invalid-id";
        var request = new UpdateUserRequest { FirstName = "Updated" };

        var result = await _controller.UpdateUser(invalidId, request);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be(AppConstants.ValidationMessages.InvalidObjectId);

        _userServiceMock.Verify(x => x.UpdateUserAsync(It.IsAny<string>(), It.IsAny<UpdateUserRequest>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var userId = "507f1f77bcf86cd799439012";
        var request = new UpdateUserRequest { FirstName = "Updated" };

        _userServiceMock
            .Setup(x => x.UpdateUserAsync(userId, request))
            .ThrowsAsync(new NotFoundException(AppConstants.ErrorMessages.UserNotFound));

        Func<Task> act = async () => await _controller.UpdateUser(userId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.UserNotFound);

        _userServiceMock.Verify(x => x.UpdateUserAsync(userId, request), Times.Once);
    }

    #endregion
}
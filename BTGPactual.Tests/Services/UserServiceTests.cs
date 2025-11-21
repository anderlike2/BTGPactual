using AutoMapper;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Implementations;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Shared.Constants;
using BTGPactual.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BTGPactual.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFundRepository> _fundRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _fundRepositoryMock = new Mock<IFundRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _mapper = AutoMapperFactory.Create();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _fundRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _mapper,
            _loggerMock.Object
        );
    }

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        var users = new List<User>
        {
            new User
            {
                Id = "507f1f77bcf86cd799439011",
                Username = "user1",
                Email = "user1@example.com",
                Role = UserRole.Client,
                IsActive = true
            },
            new User
            {
                Id = "507f1f77bcf86cd799439012",
                Username = "user2",
                Email = "user2@example.com",
                Role = UserRole.Admin,
                IsActive = true
            }
        };

        _userRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Username == "user1");
        result.Should().Contain(u => u.Username == "user2");

        _userRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithNoUsers_ShouldReturnEmptyList()
    {
        _userRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<User>());

        var result = await _userService.GetAllUsersAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUserResponse()
    {
        var userId = "507f1f77bcf86cd799439011";
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Balance = 500000m,
            Role = UserRole.Client,
            NotificationPreference = NotificationType.Email,
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var result = await _userService.GetUserByIdAsync(userId);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
        result.Balance.Should().Be(500000m);
        result.Role.Should().Be(UserRole.Client);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var userId = "507f1f77bcf86cd799439011";

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _userService.GetUserByIdAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.UserNotFound);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithValidRequest_ShouldUpdateUser()
    {
        var userId = "507f1f77bcf86cd799439011";
        var existingUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Old",
            LastName = "Name",
            PhoneNumber = "+573001234567",
            NotificationPreference = NotificationType.Email,
            IsActive = true
        };

        var request = new UpdateUserRequest
        {
            FirstName = "New",
            LastName = "Name",
            PhoneNumber = "+573009876543",
            NotificationPreference = NotificationType.SMS,
            IsActive = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(existingUser);

        var result = await _userService.UpdateUserAsync(userId, request);

        result.Should().NotBeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
            u.Id == userId &&
            u.FirstName == request.FirstName &&
            u.LastName == request.LastName &&
            u.PhoneNumber == request.PhoneNumber &&
            u.NotificationPreference == request.NotificationPreference &&
            u.IsActive == request.IsActive
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var userId = "507f1f77bcf86cd799439011";
        var request = new UpdateUserRequest { FirstName = "New Name" };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _userService.UpdateUserAsync(userId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.UserNotFound);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_WithPartialUpdate_ShouldOnlyUpdateProvidedFields()
    {
        var userId = "507f1f77bcf86cd799439011";
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Old",
            LastName = "Name",
            PhoneNumber = "+573001234567",
            NotificationPreference = NotificationType.Email,
            IsActive = true
        };

        var request = new UpdateUserRequest
        {
            FirstName = "New"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(existingUser);

        var result = await _userService.UpdateUserAsync(userId, request);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
            u.FirstName == "New" &&
            u.LastName == "Name" &&
            u.PhoneNumber == "+573001234567" &&
            u.NotificationPreference == NotificationType.Email &&
            u.IsActive == true
        )), Times.Once);
    }

    #endregion
}
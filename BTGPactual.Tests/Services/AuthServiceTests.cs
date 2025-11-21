using AutoMapper;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Implementations;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using BTGPactual.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BTGPactual.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _mapper = AutoMapperFactory.Create();

        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-for-testing-purposes-only",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object,
            _mapper,
            _loggerMock.Object,
            Options.Create(_jwtSettings)
        );
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldCreateUserAndReturnAuthResponse()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+573001234567",
            NotificationPreference = NotificationType.Email
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.HashPassword(request.Password))
            .Returns("hashed-password");

        var createdUser = new User
        {
            Id = "507f1f77bcf86cd799439011",
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = "hashed-password",
            Role = UserRole.Client,
            Balance = AppConstants.DefaultValues.InitialBalance,
            NotificationPreference = request.NotificationPreference,
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt-token");
        result.UserId.Should().Be(createdUser.Id);
        result.Username.Should().Be(createdUser.Username);
        result.Email.Should().Be(createdUser.Email);
        result.Role.Should().Be(UserRole.Client);

        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.Username == request.Username &&
            u.Email == request.Email &&
            u.Balance == AppConstants.DefaultValues.InitialBalance &&
            u.Role == UserRole.Client
        )), Times.Once);

        _passwordHasherMock.Verify(x => x.HashPassword(request.Password), Times.Once);
        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowBusinessException()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "existing@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var existingUser = new User { Email = request.Email };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        Func<Task> act = async () => await _authService.RegisterAsync(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.UserAlreadyExists);

        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldThrowBusinessException()
    {
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        var existingUser = new User { Username = request.Username };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync(existingUser);

        Func<Task> act = async () => await _authService.RegisterAsync(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.UserAlreadyExists);

        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        var user = new User
        {
            Id = "507f1f77bcf86cd799439011",
            Username = "testuser",
            Email = request.Email,
            PasswordHash = "hashed-password",
            Role = UserRole.Client,
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns("test-jwt-token");

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt-token");
        result.UserId.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(UserRole.Client);

        _passwordHasherMock.Verify(x => x.VerifyPassword(request.Password, user.PasswordHash), Times.Once);
        _tokenServiceMock.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowBusinessException()
    {
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Test123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _authService.LoginAsync(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.InvalidCredentials);

        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ShouldThrowBusinessException()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };

        var user = new User
        {
            Email = request.Email,
            PasswordHash = "hashed-password",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        Func<Task> act = async () => await _authService.LoginAsync(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.InvalidCredentials);

        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowBusinessException()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        var user = new User
        {
            Email = request.Email,
            PasswordHash = "hashed-password",
            IsActive = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        Func<Task> act = async () => await _authService.LoginAsync(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.UserNotActive);

        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    #endregion
}
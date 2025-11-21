using BTGPactual.API.Controllers;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BTGPactual.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnOkWithAuthResponse()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            NotificationPreference = NotificationType.Email
        };

        var authResponse = new AuthResponse
        {
            Token = "test-token",
            UserId = "507f1f77bcf86cd799439011",
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.Client,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(authResponse);

        var result = await _controller.Register(request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);

        var response = objectResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be(AppConstants.SuccessMessages.RegistrationSuccessful);
        response.Data.Should().NotBeNull();
        response.Data.Token.Should().Be("test-token");
        response.Data.Username.Should().Be("testuser");

        _authServiceMock.Verify(x => x.RegisterAsync(request), Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "existing@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new BusinessException(AppConstants.ErrorMessages.UserAlreadyExists));

        Func<Task> act = async () => await _controller.Register(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.UserAlreadyExists);

        _authServiceMock.Verify(x => x.RegisterAsync(request), Times.Once);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithAuthResponse()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        var authResponse = new AuthResponse
        {
            Token = "test-token",
            UserId = "507f1f77bcf86cd799439011",
            Username = "testuser",
            Email = "test@example.com",
            Role = UserRole.Client,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(authResponse);

        var result = await _controller.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be(AppConstants.SuccessMessages.LoginSuccessful);
        response.Data.Should().NotBeNull();
        response.Data.Token.Should().Be("test-token");

        _authServiceMock.Verify(x => x.LoginAsync(request), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldThrowBusinessException()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new BusinessException(AppConstants.ErrorMessages.InvalidCredentials));

        Func<Task> act = async () => await _controller.Login(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.InvalidCredentials);

        _authServiceMock.Verify(x => x.LoginAsync(request), Times.Once);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldThrowBusinessException()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new BusinessException(AppConstants.ErrorMessages.UserNotActive));

        Func<Task> act = async () => await _controller.Login(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.UserNotActive);

        _authServiceMock.Verify(x => x.LoginAsync(request), Times.Once);
    }

    #endregion
}
using AutoMapper;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BTGPactual.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Intentando registrar usuario: {Email}", request.Email);

        User? existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Intento de registro con email existente: {Email}", request.Email);
            throw new BusinessException(AppConstants.ErrorMessages.UserAlreadyExists);
        }

        User? existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUsername != null)
        {
            _logger.LogWarning("Intento de registro con username existente: {Username}", request.Username);
            throw new BusinessException(AppConstants.ErrorMessages.UserAlreadyExists);
        }

        User user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.Client,
            Balance = AppConstants.DefaultValues.InitialBalance,
            NotificationPreference = request.NotificationPreference,
            IsActive = true
        };

        User createdUser = await _userRepository.CreateAsync(user);

        _logger.LogInformation("Usuario registrado exitosamente: {UserId} - {Email}", createdUser.Id, createdUser.Email);

        string token = _tokenService.GenerateToken(createdUser);

        return new AuthResponse
        {
            Token = token,
            UserId = createdUser.Id,
            Username = createdUser.Username,
            Email = createdUser.Email,
            Role = createdUser.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Intento de login: {Email}", request.Email);

        User? user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login fallido - usuario no encontrado: {Email}", request.Email);
            throw new BusinessException(AppConstants.ErrorMessages.InvalidCredentials);
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login fallido - contraseña incorrecta: {Email}", request.Email);
            throw new BusinessException(AppConstants.ErrorMessages.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login fallido - usuario inactivo: {Email}", request.Email);
            throw new BusinessException(AppConstants.ErrorMessages.UserNotActive);
        }

        _logger.LogInformation("Login exitoso: {UserId} - {Email}", user.Id, user.Email);

        string token = _tokenService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        };
    }
}
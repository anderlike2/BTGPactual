using AutoMapper;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace BTGPactual.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFundRepository _fundRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IFundRepository fundRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _fundRepository = fundRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        _logger.LogInformation("Obteniendo todos los usuarios");

        IEnumerable<User> users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserResponse>>(users);
    }

    public async Task<UserResponse> GetUserByIdAsync(string userId)
    {
        _logger.LogInformation("Obteniendo usuario por ID: {UserId}", userId);

        User? user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("Usuario no encontrado: {UserId}", userId);
            throw new NotFoundException(AppConstants.ErrorMessages.UserNotFound);
        }

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        _logger.LogInformation("Actualizando usuario: {UserId}", userId);

        User? user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("Usuario no encontrado para actualizar: {UserId}", userId);
            throw new NotFoundException(AppConstants.ErrorMessages.UserNotFound);
        }

        if (!string.IsNullOrEmpty(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.NotificationPreference.HasValue)
        {
            user.NotificationPreference = request.NotificationPreference.Value;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        User updatedUser = await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Usuario actualizado exitosamente: {UserId}", userId);

        return _mapper.Map<UserResponse>(updatedUser);
    }
}
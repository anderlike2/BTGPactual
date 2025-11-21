using AutoMapper;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace BTGPactual.Application.Services.Implementations;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFundRepository _fundRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IUserRepository userRepository,
        IFundRepository fundRepository,
        INotificationService notificationService,
        IMapper mapper,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _fundRepository = fundRepository;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionResponse> SubscribeToFundAsync(string userId, SubscribeFundRequest request)
    {
        _logger.LogInformation("Usuario {UserId} intenta suscribirse al fondo {FundId} con monto {Amount}",
        userId, request.FundId, request.Amount);

        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Suscripción fallida - usuario no encontrado: {UserId}", userId);
            throw new NotFoundException(AppConstants.ErrorMessages.UserNotFound);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Suscripción fallida - usuario inactivo: {UserId}", userId);
            throw new BusinessException(AppConstants.ErrorMessages.UserNotActive);
        }

        Fund? fund = await _fundRepository.GetByIdAsync(request.FundId);
        if (fund == null)
        {
            _logger.LogWarning("Suscripción fallida - fondo no encontrado: {FundId}", request.FundId);
            throw new NotFoundException(AppConstants.ErrorMessages.FundNotFound);
        }

        if (!fund.IsActive)
        {
            _logger.LogWarning("Suscripción fallida - fondo inactivo: {FundId} - {FundName}", fund.Id, fund.Name);
            throw new BusinessException(AppConstants.ErrorMessages.FundNotActive);
        }

        if (request.Amount < fund.MinimumAmount)
        {
            _logger.LogWarning(
                "Suscripción fallida - monto insuficiente. Usuario: {UserId}, Fondo: {FundId}, Monto: {Amount}, Mínimo: {MinimumAmount}",
                userId, request.FundId, request.Amount, fund.MinimumAmount);

            throw new BusinessException(AppConstants.ErrorMessages.InsufficientAmount);
        }

        if (user.Balance < request.Amount)
        {
            _logger.LogWarning(
                "Suscripción fallida - balance insuficiente. Usuario: {UserId}, Balance: {Balance}, Monto requerido: {Amount}",
                userId, user.Balance, request.Amount);

            throw new BusinessException(
                $"Balance insuficiente. Balance actual: {user.Balance:C}, Monto: {request.Amount:C}, Fondo: {fund.Name}");
        }

        Transaction? existingSubscription = await _transactionRepository.GetActiveSubscriptionAsync(userId, request.FundId);

        if (existingSubscription != null)
        {
            _logger.LogWarning("Suscripción fallida - ya suscrito. Usuario: {UserId}, Fondo: {FundId}", userId, request.FundId);
            throw new BusinessException(AppConstants.ErrorMessages.AlreadySubscribed);
        }

        user.Balance -= request.Amount;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        Transaction transaction = new Transaction
        {
            UserId = userId,
            FundId = fund.Id,
            Type = TransactionType.Subscription,
            Amount = request.Amount,
            TransactionDate = DateTime.UtcNow
        };

        Transaction createdTransaction = await _transactionRepository.CreateAsync(transaction);

        _logger.LogInformation(
            "Suscripción exitosa. TransactionId: {TransactionId}, Usuario: {UserId}, Fondo: {FundId}, Monto: {Amount}, Nuevo balance: {Balance}",
            createdTransaction.Id, userId, fund.Id, request.Amount, user.Balance);

        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifySubscriptionAsync(user, fund, createdTransaction);
                _logger.LogInformation("Notificación de suscripción enviada. TransactionId: {TransactionId}", createdTransaction.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de suscripción. TransactionId: {TransactionId}", createdTransaction.Id);
            }
        });

        TransactionResponse response = _mapper.Map<TransactionResponse>(createdTransaction);
        response.FundName = fund.Name;

        return response;
    }

    public async Task<TransactionResponse> CancelSubscriptionAsync(string userId, CancelSubscriptionRequest request)
    {
        _logger.LogInformation("Usuario {UserId} intenta cancelar suscripción al fondo {FundId}", userId, request.FundId);

        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Cancelación fallida - usuario no encontrado: {UserId}", userId);
            throw new NotFoundException(AppConstants.ErrorMessages.UserNotFound);
        }

        Fund? fund = await _fundRepository.GetByIdAsync(request.FundId);
        if (fund == null)
        {
            _logger.LogWarning("Cancelación fallida - fondo no encontrado: {FundId}", request.FundId);
            throw new NotFoundException(AppConstants.ErrorMessages.FundNotFound);
        }

        Transaction? existingSubscription = await _transactionRepository.GetActiveSubscriptionAsync(userId, request.FundId);

        if (existingSubscription == null)
        {
            _logger.LogWarning("Cancelación fallida - no está suscrito. Usuario: {UserId}, Fondo: {FundId}", userId, request.FundId);
            throw new BusinessException(AppConstants.ErrorMessages.NotSubscribed);
        }

        user.Balance += existingSubscription.Amount;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        Transaction transaction = new Transaction
        {
            UserId = userId,
            FundId = fund.Id,
            Type = TransactionType.Cancellation,
            Amount = existingSubscription.Amount,
            TransactionDate = DateTime.UtcNow,
            CancellationDate = DateTime.UtcNow
        };

        Transaction createdTransaction = await _transactionRepository.CreateAsync(transaction);

        _logger.LogInformation(
            "Cancelación exitosa. TransactionId: {TransactionId}, Usuario: {UserId}, Fondo: {FundId}, Monto devuelto: {Amount}, Nuevo balance: {Balance}",
            createdTransaction.Id, userId, fund.Id, existingSubscription.Amount, user.Balance);

        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyCancellationAsync(user, fund, createdTransaction);
                _logger.LogInformation("Notificación de cancelación enviada. TransactionId: {TransactionId}", createdTransaction.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de cancelación. TransactionId: {TransactionId}", createdTransaction.Id);
            }
        });

        TransactionResponse response = _mapper.Map<TransactionResponse>(createdTransaction);
        response.FundName = fund.Name;

        return response;
    }

    public async Task<IEnumerable<TransactionResponse>> GetClientTransactionsAsync(string userId)
    {
        _logger.LogInformation("Obteniendo transacciones del usuario: {UserId}", userId);

        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Usuario no encontrado: {UserId}", userId);
            throw new NotFoundException(AppConstants.ErrorMessages.UserNotFound);
        }

        IEnumerable<Transaction> transactions = await _transactionRepository.GetByUserIdAsync(userId);

        List<TransactionResponse> transactionResponses = _mapper.Map<IEnumerable<TransactionResponse>>(transactions).ToList();

        foreach (TransactionResponse transactionResponse in transactionResponses)
        {
            Fund? fund = await _fundRepository.GetByIdAsync(transactionResponse.FundId);
            if (fund != null)
            {
                transactionResponse.FundName = fund.Name;
            }
        }

        return transactionResponses;
    }

    public async Task<IEnumerable<TransactionResponse>> GetAllTransactionsAsync()
    {
        _logger.LogInformation("Obteniendo todas las transacciones del sistema");

        IEnumerable<Transaction> transactions = await _transactionRepository.GetAllAsync();

        List<TransactionResponse> transactionResponses = _mapper.Map<IEnumerable<TransactionResponse>>(transactions).ToList();

        foreach (TransactionResponse transactionResponse in transactionResponses)
        {
            Fund? fund = await _fundRepository.GetByIdAsync(transactionResponse.FundId);
            if (fund != null)
            {
                transactionResponse.FundName = fund.Name;
            }
        }

        return transactionResponses;
    }
}
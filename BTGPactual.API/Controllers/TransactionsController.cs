using System.Security.Claims;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTGPactual.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Suscribirse a un fondo (solo Cliente)
    /// </summary>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(ApiResponse<TransactionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubscribeToFund([FromBody] SubscribeFundRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(
                AppConstants.ErrorMessages.Unauthorized));
        }

        TransactionResponse transaction = await _transactionService.SubscribeToFundAsync(userId, request);
        ApiResponse<TransactionResponse> response = ApiResponse<TransactionResponse>.SuccessResponse(
            transaction,
            AppConstants.SuccessMessages.SubscriptionSuccessful);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Cancelar suscripción a un fondo (solo Cliente)
    /// </summary>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(ApiResponse<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(
                AppConstants.ErrorMessages.Unauthorized));
        }

        TransactionResponse transaction = await _transactionService.CancelSubscriptionAsync(userId, request);
        ApiResponse<TransactionResponse> response = ApiResponse<TransactionResponse>.SuccessResponse(
            transaction,
            AppConstants.SuccessMessages.CancellationSuccessful);

        return Ok(response);
    }

    /// <summary>
    /// Obtener mis transacciones (solo Cliente - sus propias transacciones)
    /// </summary>
    [HttpGet("my-transactions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTransactions()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(
                AppConstants.ErrorMessages.Unauthorized));
        }

        IEnumerable<TransactionResponse> transactions = await _transactionService.GetClientTransactionsAsync(userId);
        ApiResponse<IEnumerable<TransactionResponse>> response = ApiResponse<IEnumerable<TransactionResponse>>.SuccessResponse(transactions);

        return Ok(response);
    }
}
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Extensions;
using BTGPactual.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTGPactual.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITransactionService _transactionService;

    public AdminController(
        IUserService userService,
        ITransactionService transactionService)
    {
        _userService = userService;
        _transactionService = transactionService;
    }

    #region User Management

    /// <summary>
    /// Obtener todos los usuarios (solo Admin)
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        IEnumerable<UserResponse> users = await _userService.GetAllUsersAsync();
        ApiResponse<IEnumerable<UserResponse>> response = ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(users);
        return Ok(response);
    }

    /// <summary>
    /// Obtener un usuario por ID (solo Admin)
    /// </summary>
    [HttpGet("users/{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(string id)
    {
        if (!id.IsValidObjectId())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(AppConstants.ValidationMessages.InvalidObjectId));
        }
        UserResponse user = await _userService.GetUserByIdAsync(id);
        ApiResponse<UserResponse> response = ApiResponse<UserResponse>.SuccessResponse(user);
        return Ok(response);
    }

    /// <summary>
    /// Actualizar información de un usuario (solo Admin)
    /// </summary>
    [HttpPut("users/{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        if (!id.IsValidObjectId())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(AppConstants.ValidationMessages.InvalidObjectId));
        }
        UserResponse user = await _userService.UpdateUserAsync(id, request);
        ApiResponse<UserResponse> response = ApiResponse<UserResponse>.SuccessResponse(user);
        return Ok(response);
    }

    #endregion

    #region Transaction Management

    /// <summary>
    /// Obtener todas las transacciones del sistema (solo Admin)
    /// </summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllTransactions()
    {
        IEnumerable<TransactionResponse> transactions = await _transactionService.GetAllTransactionsAsync();
        ApiResponse<IEnumerable<TransactionResponse>> response = ApiResponse<IEnumerable<TransactionResponse>>.SuccessResponse(transactions);
        return Ok(response);
    }

    /// <summary>
    /// Obtener transacciones de un usuario específico (solo Admin)
    /// </summary>
    [HttpGet("transactions/user/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserTransactions(string userId)
    {
        if (!userId.IsValidObjectId())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(AppConstants.ValidationMessages.InvalidObjectId));
        }
        IEnumerable<TransactionResponse> transactions = await _transactionService.GetClientTransactionsAsync(userId);
        ApiResponse<IEnumerable<TransactionResponse>> response = ApiResponse<IEnumerable<TransactionResponse>>.SuccessResponse(transactions);
        return Ok(response);
    }

    #endregion
}
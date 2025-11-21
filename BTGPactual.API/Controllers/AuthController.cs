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
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registrar un nuevo usuario
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        AuthResponse result = await _authService.RegisterAsync(request);
        ApiResponse<AuthResponse> response = ApiResponse<AuthResponse>.SuccessResponse(
            result,
            AppConstants.SuccessMessages.RegistrationSuccessful);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Iniciar sesión
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        AuthResponse result = await _authService.LoginAsync(request);
        ApiResponse<AuthResponse> response = ApiResponse<AuthResponse>.SuccessResponse(
            result,
            AppConstants.SuccessMessages.LoginSuccessful);
        return Ok(response);
    }
}
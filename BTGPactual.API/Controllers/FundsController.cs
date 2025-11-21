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
public class FundsController : ControllerBase
{
    private readonly IFundService _fundService;

    public FundsController(IFundService fundService)
    {
        _fundService = fundService;
    }

    /// <summary>
    /// Obtener todos los fondos disponibles (público)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FundResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFunds()
    {
        IEnumerable<FundResponse> funds = await _fundService.GetAllFundsAsync();
        ApiResponse<IEnumerable<FundResponse>> response = ApiResponse<IEnumerable<FundResponse>>.SuccessResponse(funds);
        return Ok(response);
    }

    /// <summary>
    /// Obtener un fondo por ID (público)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FundResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFundById(string id)
    {
        if (!id.IsValidObjectId())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(AppConstants.ValidationMessages.InvalidObjectId));
        }
        FundResponse fund = await _fundService.GetFundByIdAsync(id);
        ApiResponse<FundResponse> response = ApiResponse<FundResponse>.SuccessResponse(fund);
        return Ok(response);
    }

    /// <summary>
    /// Crear un nuevo fondo (solo Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<FundResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateFund([FromBody] CreateFundRequest request)
    {
        FundResponse fund = await _fundService.CreateFundAsync(request);
        ApiResponse<FundResponse> response = ApiResponse<FundResponse>.SuccessResponse(
            fund,
            AppConstants.SuccessMessages.FundCreated);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Actualizar un fondo existente (solo Admin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<FundResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateFund(string id, [FromBody] UpdateFundRequest request)
    {
        if (!id.IsValidObjectId())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(AppConstants.ValidationMessages.InvalidObjectId));
        }
        FundResponse fund = await _fundService.UpdateFundAsync(id, request);
        ApiResponse<FundResponse> response = ApiResponse<FundResponse>.SuccessResponse(
            fund,
            AppConstants.SuccessMessages.FundUpdated);
        return Ok(response);
    }
}
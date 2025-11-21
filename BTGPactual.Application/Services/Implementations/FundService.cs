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

public class FundService : IFundService
{
    private readonly IFundRepository _fundRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FundService> _logger;

    public FundService(
        IFundRepository fundRepository,
        IMapper mapper,
        ILogger<FundService> logger)
    {
        _fundRepository = fundRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundResponse> GetFundByIdAsync(string fundId)
    {
        _logger.LogInformation("Obteniendo fondo por ID: {FundId}", fundId);

        Fund? fund = await _fundRepository.GetByIdAsync(fundId);

        if (fund == null)
        {
            _logger.LogWarning("Fondo no encontrado: {FundId}", fundId);
            throw new NotFoundException(AppConstants.ErrorMessages.FundNotFound);
        }

        return _mapper.Map<FundResponse>(fund);
    }

    public async Task<IEnumerable<FundResponse>> GetAllFundsAsync()
    {
        _logger.LogInformation("Obteniendo todos los fondos");

        IEnumerable<Fund> funds = await _fundRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<FundResponse>>(funds);
    }

    public async Task<FundResponse> CreateFundAsync(CreateFundRequest request)
    {
        _logger.LogInformation("Creando nuevo fondo: {FundName}", request.Name);

        Fund? existingFund = await _fundRepository.GetByNameAsync(request.Name);
        if (existingFund != null)
        {
            _logger.LogWarning("Intento de crear fondo con nombre existente: {FundName}", request.Name);
            throw new BusinessException(AppConstants.ErrorMessages.FundAlreadyExists);
        }

        Fund fund = new Fund
        {
            Name = request.Name,
            MinimumAmount = request.MinimumAmount,
            Category = request.Category,
            Description = request.Description,
            IsActive = true
        };

        Fund createdFund = await _fundRepository.CreateAsync(fund);

        _logger.LogInformation("Fondo creado exitosamente: {FundId} - {FundName}", createdFund.Id, createdFund.Name);

        return _mapper.Map<FundResponse>(createdFund);
    }

    public async Task<FundResponse> UpdateFundAsync(string fundId, UpdateFundRequest request)
    {
        _logger.LogInformation("Actualizando fondo: {FundId}", fundId);

        Fund? fund = await _fundRepository.GetByIdAsync(fundId);

        if (fund == null)
        {
            _logger.LogWarning("Fondo no encontrado para actualizar: {FundId}", fundId);
            throw new NotFoundException(AppConstants.ErrorMessages.FundNotFound);
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            fund.Name = request.Name;
        }

        if (request.MinimumAmount.HasValue)
        {
            fund.MinimumAmount = request.MinimumAmount.Value;
        }

        if (request.Category.HasValue)
        {
            fund.Category = request.Category.Value;
        }

        if (!string.IsNullOrEmpty(request.Description))
        {
            fund.Description = request.Description;
        }

        if (request.IsActive.HasValue)
        {
            fund.IsActive = request.IsActive.Value;
        }

        fund.UpdatedAt = DateTime.UtcNow;

        Fund updatedFund = await _fundRepository.UpdateAsync(fund);

        _logger.LogInformation("Fondo actualizado exitosamente: {FundId} - {FundName}", updatedFund.Id, updatedFund.Name);

        return _mapper.Map<FundResponse>(updatedFund);
    }
}
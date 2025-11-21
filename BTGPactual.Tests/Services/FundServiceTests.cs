using AutoMapper;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Implementations;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Shared.Constants;
using BTGPactual.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BTGPactual.Tests.Services;

public class FundServiceTests
{
    private readonly Mock<IFundRepository> _fundRepositoryMock;
    private readonly Mock<ILogger<FundService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly FundService _fundService;

    public FundServiceTests()
    {
        _fundRepositoryMock = new Mock<IFundRepository>();
        _loggerMock = new Mock<ILogger<FundService>>();
        _mapper = AutoMapperFactory.Create();

        _fundService = new FundService(
            _fundRepositoryMock.Object,
            _mapper,
            _loggerMock.Object
        );
    }

    #region GetFundByIdAsync Tests

    [Fact]
    public async Task GetFundByIdAsync_WithValidId_ShouldReturnFundResponse()
    {
        var fundId = "507f1f77bcf86cd799439011";
        var fund = new Fund
        {
            Id = fundId,
            Name = "Test Fund",
            MinimumAmount = 100000m,
            Category = FundCategory.FPV,
            IsActive = true,
            Description = "Test Description"
        };

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        var result = await _fundService.GetFundByIdAsync(fundId);

        result.Should().NotBeNull();
        result.Id.Should().Be(fundId);
        result.Name.Should().Be("Test Fund");
        result.MinimumAmount.Should().Be(100000m);
        result.Category.Should().Be(FundCategory.FPV);
        result.IsActive.Should().BeTrue();

        _fundRepositoryMock.Verify(x => x.GetByIdAsync(fundId), Times.Once);
    }

    [Fact]
    public async Task GetFundByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var fundId = "507f1f77bcf86cd799439011";

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync((Fund?)null);

        Func<Task> act = async () => await _fundService.GetFundByIdAsync(fundId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.FundNotFound);
    }

    #endregion

    #region GetAllFundsAsync Tests

    [Fact]
    public async Task GetAllFundsAsync_ShouldReturnAllFunds()
    {
        var funds = new List<Fund>
        {
            new Fund
            {
                Id = "507f1f77bcf86cd799439011",
                Name = "Fund 1",
                MinimumAmount = 100000m,
                Category = FundCategory.FPV,
                IsActive = true
            },
            new Fund
            {
                Id = "507f1f77bcf86cd799439012",
                Name = "Fund 2",
                MinimumAmount = 200000m,
                Category = FundCategory.FIC,
                IsActive = false
            }
        };

        _fundRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(funds);

        var result = await _fundService.GetAllFundsAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Name == "Fund 1");
        result.Should().Contain(f => f.Name == "Fund 2");

        _fundRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllFundsAsync_WithNoFunds_ShouldReturnEmptyList()
    {
        _fundRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Fund>());

        var result = await _fundService.GetAllFundsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateFundAsync Tests

    [Fact]
    public async Task CreateFundAsync_WithValidRequest_ShouldCreateFund()
    {
        var request = new CreateFundRequest
        {
            Name = "New Fund",
            MinimumAmount = 150000m,
            Category = FundCategory.FPV,
            Description = "New Fund Description"
        };

        _fundRepositoryMock
            .Setup(x => x.GetByNameAsync(request.Name))
            .ReturnsAsync((Fund?)null);

        var createdFund = new Fund
        {
            Id = "507f1f77bcf86cd799439011",
            Name = request.Name,
            MinimumAmount = request.MinimumAmount,
            Category = request.Category,
            Description = request.Description,
            IsActive = true
        };

        _fundRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Fund>()))
            .ReturnsAsync(createdFund);

        var result = await _fundService.CreateFundAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().Be(createdFund.Id);
        result.Name.Should().Be(request.Name);
        result.MinimumAmount.Should().Be(request.MinimumAmount);
        result.Category.Should().Be(request.Category);
        result.IsActive.Should().BeTrue();

        _fundRepositoryMock.Verify(x => x.CreateAsync(It.Is<Fund>(f =>
            f.Name == request.Name &&
            f.MinimumAmount == request.MinimumAmount &&
            f.IsActive == true
        )), Times.Once);
    }

    [Fact]
    public async Task CreateFundAsync_WithExistingName_ShouldThrowBusinessException()
    {
        var request = new CreateFundRequest
        {
            Name = "Existing Fund",
            MinimumAmount = 150000m,
            Category = FundCategory.FPV
        };

        var existingFund = new Fund { Name = request.Name };

        _fundRepositoryMock
            .Setup(x => x.GetByNameAsync(request.Name))
            .ReturnsAsync(existingFund);

        Func<Task> act = async () => await _fundService.CreateFundAsync(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.FundAlreadyExists);

        _fundRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Fund>()), Times.Never);
    }

    #endregion

    #region UpdateFundAsync Tests

    [Fact]
    public async Task UpdateFundAsync_WithValidRequest_ShouldUpdateFund()
    {
        var fundId = "507f1f77bcf86cd799439011";
        var existingFund = new Fund
        {
            Id = fundId,
            Name = "Old Name",
            MinimumAmount = 100000m,
            Category = FundCategory.FPV,
            IsActive = true
        };

        var request = new UpdateFundRequest
        {
            Name = "New Name",
            MinimumAmount = 150000m,
            Category = FundCategory.FIC,
            IsActive = false
        };

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(existingFund);

        _fundRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Fund>()))
            .ReturnsAsync(existingFund);

        var result = await _fundService.UpdateFundAsync(fundId, request);

        result.Should().NotBeNull();

        _fundRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Fund>(f =>
            f.Id == fundId &&
            f.Name == request.Name &&
            f.MinimumAmount == request.MinimumAmount &&
            f.Category == request.Category &&
            f.IsActive == request.IsActive
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateFundAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var fundId = "507f1f77bcf86cd799439011";
        var request = new UpdateFundRequest { Name = "New Name" };

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync((Fund?)null);

        Func<Task> act = async () => await _fundService.UpdateFundAsync(fundId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.FundNotFound);

        _fundRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fund>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFundAsync_WithPartialUpdate_ShouldOnlyUpdateProvidedFields()
    {
        var fundId = "507f1f77bcf86cd799439011";
        var existingFund = new Fund
        {
            Id = fundId,
            Name = "Old Name",
            MinimumAmount = 100000m,
            Category = FundCategory.FPV,
            IsActive = true
        };

        var request = new UpdateFundRequest
        {
            Name = "New Name"
        };

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(existingFund);

        _fundRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Fund>()))
            .ReturnsAsync(existingFund);

        var result = await _fundService.UpdateFundAsync(fundId, request);

        _fundRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Fund>(f =>
            f.Name == "New Name" &&
            f.MinimumAmount == 100000m &&
            f.Category == FundCategory.FPV &&
            f.IsActive == true
        )), Times.Once);
    }

    #endregion
}
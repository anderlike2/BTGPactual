using System.Security.Claims;
using BTGPactual.API.Controllers;
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.DTOs.Responses;
using BTGPactual.Application.Exceptions;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BTGPactual.Tests.Controllers;

public class FundsControllerTests
{
    private readonly Mock<IFundService> _fundServiceMock;
    private readonly FundsController _controller;

    public FundsControllerTests()
    {
        _fundServiceMock = new Mock<IFundService>();
        _controller = new FundsController(_fundServiceMock.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "507f1f77bcf86cd799439011"),
            new Claim(ClaimTypes.Role, UserRole.Admin.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetAllFunds Tests

    [Fact]
    public async Task GetAllFunds_ShouldReturnOkWithFundsList()
    {
        var funds = new List<FundResponse>
        {
            new FundResponse
            {
                Id = "507f1f77bcf86cd799439011",
                Name = "Fund 1",
                MinimumAmount = 100000m,
                Category = FundCategory.FPV,
                IsActive = true
            },
            new FundResponse
            {
                Id = "507f1f77bcf86cd799439012",
                Name = "Fund 2",
                MinimumAmount = 200000m,
                Category = FundCategory.FIC,
                IsActive = true
            }
        };

        _fundServiceMock
            .Setup(x => x.GetAllFundsAsync())
            .ReturnsAsync(funds);

        var result = await _controller.GetAllFunds();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<FundResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        _fundServiceMock.Verify(x => x.GetAllFundsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllFunds_WithNoFunds_ShouldReturnEmptyList()
    {
        _fundServiceMock
            .Setup(x => x.GetAllFundsAsync())
            .ReturnsAsync(new List<FundResponse>());

        var result = await _controller.GetAllFunds();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<FundResponse>>>().Subject;
        response.Data.Should().BeEmpty();
    }

    #endregion

    #region GetFundById Tests

    [Fact]
    public async Task GetFundById_WithValidId_ShouldReturnOkWithFund()
    {
        var fundId = "507f1f77bcf86cd799439011";
        var fund = new FundResponse
        {
            Id = fundId,
            Name = "Test Fund",
            MinimumAmount = 100000m,
            Category = FundCategory.FPV,
            IsActive = true
        };

        _fundServiceMock
            .Setup(x => x.GetFundByIdAsync(fundId))
            .ReturnsAsync(fund);

        var result = await _controller.GetFundById(fundId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<FundResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(fundId);

        _fundServiceMock.Verify(x => x.GetFundByIdAsync(fundId), Times.Once);
    }

    [Fact]
    public async Task GetFundById_WithInvalidId_ShouldReturnBadRequest()
    {
        var invalidId = "invalid-id";

        var result = await _controller.GetFundById(invalidId);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be(AppConstants.ValidationMessages.InvalidObjectId);

        _fundServiceMock.Verify(x => x.GetFundByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetFundById_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var fundId = "507f1f77bcf86cd799439011";

        _fundServiceMock
            .Setup(x => x.GetFundByIdAsync(fundId))
            .ThrowsAsync(new NotFoundException(AppConstants.ErrorMessages.FundNotFound));

        Func<Task> act = async () => await _controller.GetFundById(fundId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.FundNotFound);

        _fundServiceMock.Verify(x => x.GetFundByIdAsync(fundId), Times.Once);
    }

    #endregion

    #region CreateFund Tests

    [Fact]
    public async Task CreateFund_WithValidRequest_ShouldReturnCreatedWithFund()
    {
        var request = new CreateFundRequest
        {
            Name = "New Fund",
            MinimumAmount = 150000m,
            Category = FundCategory.FPV,
            Description = "New Fund Description"
        };

        var createdFund = new FundResponse
        {
            Id = "507f1f77bcf86cd799439012",
            Name = request.Name,
            MinimumAmount = request.MinimumAmount,
            Category = request.Category,
            Description = request.Description,
            IsActive = true
        };

        _fundServiceMock
            .Setup(x => x.CreateFundAsync(request))
            .ReturnsAsync(createdFund);

        var result = await _controller.CreateFund(request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);

        var response = objectResult.Value.Should().BeOfType<ApiResponse<FundResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be(AppConstants.SuccessMessages.FundCreated);
        response.Data.Should().NotBeNull();
        response.Data.Name.Should().Be(request.Name);

        _fundServiceMock.Verify(x => x.CreateFundAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateFund_WithExistingName_ShouldThrowBusinessException()
    {
        var request = new CreateFundRequest
        {
            Name = "Existing Fund",
            MinimumAmount = 150000m,
            Category = FundCategory.FPV
        };

        _fundServiceMock
            .Setup(x => x.CreateFundAsync(request))
            .ThrowsAsync(new BusinessException(AppConstants.ErrorMessages.FundAlreadyExists));

        Func<Task> act = async () => await _controller.CreateFund(request);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AppConstants.ErrorMessages.FundAlreadyExists);

        _fundServiceMock.Verify(x => x.CreateFundAsync(request), Times.Once);
    }

    #endregion

    #region UpdateFund Tests

    [Fact]
    public async Task UpdateFund_WithValidRequest_ShouldReturnOkWithUpdatedFund()
    {
        var fundId = "507f1f77bcf86cd799439012";
        var request = new UpdateFundRequest
        {
            Name = "Updated Fund",
            MinimumAmount = 200000m,
            IsActive = false
        };

        var updatedFund = new FundResponse
        {
            Id = fundId,
            Name = "Updated Fund",
            MinimumAmount = 200000m,
            Category = FundCategory.FPV,
            IsActive = false
        };

        _fundServiceMock
            .Setup(x => x.UpdateFundAsync(fundId, request))
            .ReturnsAsync(updatedFund);

        var result = await _controller.UpdateFund(fundId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<ApiResponse<FundResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be(AppConstants.SuccessMessages.FundUpdated);
        response.Data.Should().NotBeNull();
        response.Data.Name.Should().Be("Updated Fund");
        response.Data.IsActive.Should().BeFalse();

        _fundServiceMock.Verify(x => x.UpdateFundAsync(fundId, request), Times.Once);
    }

    [Fact]
    public async Task UpdateFund_WithInvalidId_ShouldReturnBadRequest()
    {
        var invalidId = "invalid-id";
        var request = new UpdateFundRequest { Name = "Updated" };

        var result = await _controller.UpdateFund(invalidId, request);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be(AppConstants.ValidationMessages.InvalidObjectId);

        _fundServiceMock.Verify(x => x.UpdateFundAsync(It.IsAny<string>(), It.IsAny<UpdateFundRequest>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFund_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var fundId = "507f1f77bcf86cd799439012";
        var request = new UpdateFundRequest { Name = "Updated" };

        _fundServiceMock
            .Setup(x => x.UpdateFundAsync(fundId, request))
            .ThrowsAsync(new NotFoundException(AppConstants.ErrorMessages.FundNotFound));

        Func<Task> act = async () => await _controller.UpdateFund(fundId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AppConstants.ErrorMessages.FundNotFound);

        _fundServiceMock.Verify(x => x.UpdateFundAsync(fundId, request), Times.Once);
    }

    #endregion
}
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Validators;
using BTGPactual.Shared.Constants;
using FluentValidation.TestHelper;

namespace BTGPactual.Tests.Validators;

public class SubscribeFundRequestValidatorTests
{
    private readonly SubscribeFundRequestValidator _validator;

    public SubscribeFundRequestValidatorTests()
    {
        _validator = new SubscribeFundRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new SubscribeFundRequest
        {
            FundId = "507f1f77bcf86cd799439011",
            Amount = 100000m
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region FundId Validation

    [Fact]
    public void Validate_WithEmptyFundId_ShouldHaveValidationError()
    {
        var request = new SubscribeFundRequest
        {
            FundId = "",
            Amount = 100000m
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FundId)
            .WithErrorMessage(AppConstants.ValidationMessages.FundIdRequired);
    }

    [Fact]
    public void Validate_WithInvalidFundId_ShouldHaveValidationError()
    {
        var request = new SubscribeFundRequest
        {
            FundId = "invalid-id",
            Amount = 100000m
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FundId)
            .WithErrorMessage(AppConstants.ValidationMessages.InvalidObjectId);
    }

    #endregion

    #region Amount Validation

    [Fact]
    public void Validate_WithZeroAmount_ShouldHaveValidationError()
    {
        var request = new SubscribeFundRequest
        {
            FundId = "507f1f77bcf86cd799439011",
            Amount = 0m
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage(AppConstants.ValidationMessages.AmountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WithNegativeAmount_ShouldHaveValidationError()
    {
        var request = new SubscribeFundRequest
        {
            FundId = "507f1f77bcf86cd799439011",
            Amount = -100m
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage(AppConstants.ValidationMessages.AmountMustBeGreaterThanZero);
    }

    #endregion
}
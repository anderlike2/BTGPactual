using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Validators;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using FluentValidation.TestHelper;

namespace BTGPactual.Tests.Validators;

public class UpdateFundRequestValidatorTests
{
    private readonly UpdateFundRequestValidator _validator;

    public UpdateFundRequestValidatorTests()
    {
        _validator = new UpdateFundRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new UpdateFundRequest
        {
            Name = "Updated Fund",
            MinimumAmount = 150000m,
            Category = FundCategory.FIC,
            Description = "Updated Description",
            IsActive = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAllNullFields_ShouldNotHaveValidationErrors()
    {
        var request = new UpdateFundRequest
        {
            Name = null,
            MinimumAmount = null,
            Category = null,
            Description = null,
            IsActive = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region Name Validation

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveValidationError()
    {
        var request = new UpdateFundRequest
        {
            Name = new string('a', 101)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(AppConstants.ValidationMessages.FundNameMaxLength);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldNotHaveValidationError()
    {
        var request = new UpdateFundRequest
        {
            Name = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region MinimumAmount Validation

    [Fact]
    public void Validate_WithZeroMinimumAmount_ShouldHaveValidationError()
    {
        var request = new UpdateFundRequest
        {
            MinimumAmount = 0m
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.MinimumAmount)
            .WithErrorMessage(AppConstants.ValidationMessages.MinimumAmountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WithNegativeMinimumAmount_ShouldHaveValidationError()
    {
        var request = new UpdateFundRequest
        {
            MinimumAmount = -100m
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.MinimumAmount)
            .WithErrorMessage(AppConstants.ValidationMessages.MinimumAmountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WithPositiveMinimumAmount_ShouldNotHaveValidationError()
    {
        var request = new UpdateFundRequest
        {
            MinimumAmount = 100000m
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.MinimumAmount);
    }

    #endregion

    #region Category Validation

    [Fact]
    public void Validate_WithInvalidCategory_ShouldHaveValidationError()
    {
        var request = new UpdateFundRequest
        {
            Category = (FundCategory)999
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage(AppConstants.ValidationMessages.InvalidFundCategory);
    }

    [Theory]
    [InlineData(FundCategory.FPV)]
    [InlineData(FundCategory.FIC)]
    public void Validate_WithValidCategory_ShouldNotHaveValidationError(FundCategory category)
    {
        var request = new UpdateFundRequest
        {
            Category = category
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    #endregion
}
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Validators;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using FluentValidation.TestHelper;

namespace BTGPactual.Tests.Validators;

public class CreateFundRequestValidatorTests
{
    private readonly CreateFundRequestValidator _validator;

    public CreateFundRequestValidatorTests()
    {
        _validator = new CreateFundRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new CreateFundRequest
        {
            Name = "Test Fund",
            MinimumAmount = 100000m,
            Category = FundCategory.FPV,
            Description = "Test Description"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region Name Validation

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveValidationError()
    {
        var request = new CreateFundRequest
        {
            Name = "",
            MinimumAmount = 100000m,
            Category = FundCategory.FPV
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(AppConstants.ValidationMessages.FundNameRequired);
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveValidationError()
    {
        var request = new CreateFundRequest
        {
            Name = new string('a', 101),
            MinimumAmount = 100000m,
            Category = FundCategory.FPV
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(AppConstants.ValidationMessages.FundNameMaxLength);
    }

    #endregion

    #region MinimumAmount Validation

    [Fact]
    public void Validate_WithZeroMinimumAmount_ShouldHaveValidationError()
    {
        var request = new CreateFundRequest
        {
            Name = "Test Fund",
            MinimumAmount = 0m,
            Category = FundCategory.FPV
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.MinimumAmount)
            .WithErrorMessage(AppConstants.ValidationMessages.MinimumAmountMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WithNegativeMinimumAmount_ShouldHaveValidationError()
    {
        var request = new CreateFundRequest
        {
            Name = "Test Fund",
            MinimumAmount = -100m,
            Category = FundCategory.FPV
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.MinimumAmount)
            .WithErrorMessage(AppConstants.ValidationMessages.MinimumAmountMustBeGreaterThanZero);
    }

    #endregion

    #region Category Validation

    [Fact]
    public void Validate_WithInvalidCategory_ShouldHaveValidationError()
    {
        var request = new CreateFundRequest
        {
            Name = "Test Fund",
            MinimumAmount = 100000m,
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
        var request = new CreateFundRequest
        {
            Name = "Test Fund",
            MinimumAmount = 100000m,
            Category = category
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    #endregion
}
using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Validators;
using BTGPactual.Shared.Constants;
using FluentValidation.TestHelper;

namespace BTGPactual.Tests.Validators;

public class CancelSubscriptionRequestValidatorTests
{
    private readonly CancelSubscriptionRequestValidator _validator;

    public CancelSubscriptionRequestValidatorTests()
    {
        _validator = new CancelSubscriptionRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new CancelSubscriptionRequest
        {
            FundId = "507f1f77bcf86cd799439011"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyFundId_ShouldHaveValidationError()
    {
        var request = new CancelSubscriptionRequest
        {
            FundId = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FundId)
            .WithErrorMessage(AppConstants.ValidationMessages.FundIdRequired);
    }

    [Fact]
    public void Validate_WithInvalidFundId_ShouldHaveValidationError()
    {
        var request = new CancelSubscriptionRequest
        {
            FundId = "invalid-id"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FundId)
            .WithErrorMessage(AppConstants.ValidationMessages.InvalidObjectId);
    }
}
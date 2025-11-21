using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Validators;
using BTGPactual.Shared.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BTGPactual.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!@#"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region Email Validation

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError()
    {
        var request = new LoginRequest
        {
            Email = "",
            Password = "Test123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(AppConstants.ValidationMessages.EmailRequired);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveValidationError()
    {
        var request = new LoginRequest
        {
            Email = "invalid-email",
            Password = "Test123!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(AppConstants.ValidationMessages.EmailInvalid);
    }

    #endregion

    #region Password Validation

    [Fact]
    public void Validate_WithEmptyPassword_ShouldHaveValidationError()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(AppConstants.ValidationMessages.PasswordRequired);
    }

    [Fact]
    public void Validate_WithPasswordTooShort_ShouldHaveValidationError()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(AppConstants.ValidationMessages.PasswordMinLength);
    }

    #endregion
}
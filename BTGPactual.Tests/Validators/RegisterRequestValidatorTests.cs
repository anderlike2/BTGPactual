using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Validators;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BTGPactual.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!@#",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+573001234567",
            NotificationPreference = NotificationType.Email
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region Username Validation

    [Fact]
    public void Validate_WithEmptyUsername_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(AppConstants.ValidationMessages.UsernameRequired);
    }

    [Fact]
    public void Validate_WithUsernameTooShort_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "ab",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(AppConstants.ValidationMessages.UsernameMinLength);
    }

    [Fact]
    public void Validate_WithUsernameTooLong_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = new string('a', 51),
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage(AppConstants.ValidationMessages.UsernameMaxLength);
    }

    #endregion

    #region Email Validation

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(AppConstants.ValidationMessages.EmailRequired);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(AppConstants.ValidationMessages.EmailInvalid);
    }

    [Theory]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test@@example.com")]
    [InlineData("test.example.com")]
    public void Validate_WithVariousInvalidEmails_ShouldHaveValidationError(string email)
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = email,
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Validation

    [Fact]
    public void Validate_WithEmptyPassword_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "",
            FirstName = "Test",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(AppConstants.ValidationMessages.PasswordRequired);
    }

    [Fact]
    public void Validate_WithPasswordTooShort_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test12",
            FirstName = "Test",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(AppConstants.ValidationMessages.PasswordMinLength);
    }

    #endregion

    #region FirstName Validation

    [Fact]
    public void Validate_WithEmptyFirstName_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "",
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(AppConstants.ValidationMessages.FirstNameRequired);
    }

    [Fact]
    public void Validate_WithFirstNameTooLong_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = new string('a', 51),
            LastName = "User"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(AppConstants.ValidationMessages.FirstNameMaxLength);
    }

    #endregion

    #region LastName Validation

    [Fact]
    public void Validate_WithEmptyLastName_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(AppConstants.ValidationMessages.LastNameRequired);
    }

    [Fact]
    public void Validate_WithLastNameTooLong_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = new string('a', 51)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(AppConstants.ValidationMessages.LastNameMaxLength);
    }

    #endregion

    #region PhoneNumber Validation

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldNotHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+573001234567"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("+")]
    [InlineData("300123456")]
    [InlineData("+5730012345678901234567890")]
    public void Validate_WithInvalidPhoneNumber_ShouldHaveValidationError(string phoneNumber)
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = phoneNumber
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage(AppConstants.ValidationMessages.PhoneNumberInvalid);
    }

    [Fact]
    public void Validate_WithNullPhoneNumber_ShouldNotHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    #endregion

    #region NotificationPreference Validation

    [Fact]
    public void Validate_WithInvalidNotificationPreference_ShouldHaveValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            NotificationPreference = (NotificationType)999
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.NotificationPreference)
            .WithErrorMessage(AppConstants.ValidationMessages.InvalidNotificationPreference);
    }

    #endregion
}
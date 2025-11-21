using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Application.Validators;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using FluentValidation.TestHelper;

namespace BTGPactual.Tests.Validators;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator;

    public UpdateUserRequestValidatorTests()
    {
        _validator = new UpdateUserRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            PhoneNumber = "+573001234567",
            NotificationPreference = NotificationType.SMS,
            IsActive = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAllNullFields_ShouldNotHaveValidationErrors()
    {
        var request = new UpdateUserRequest
        {
            FirstName = null,
            LastName = null,
            PhoneNumber = null,
            NotificationPreference = null,
            IsActive = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region FirstName Validation

    [Fact]
    public void Validate_WithFirstNameTooLong_ShouldHaveValidationError()
    {
        var request = new UpdateUserRequest
        {
            FirstName = new string('a', 51)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(AppConstants.ValidationMessages.FirstNameMaxLength);
    }

    [Fact]
    public void Validate_WithEmptyFirstName_ShouldNotHaveValidationError()
    {
        var request = new UpdateUserRequest
        {
            FirstName = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    #endregion

    #region LastName Validation

    [Fact]
    public void Validate_WithLastNameTooLong_ShouldHaveValidationError()
    {
        var request = new UpdateUserRequest
        {
            LastName = new string('a', 51)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(AppConstants.ValidationMessages.LastNameMaxLength);
    }

    [Fact]
    public void Validate_WithEmptyLastName_ShouldNotHaveValidationError()
    {
        var request = new UpdateUserRequest
        {
            LastName = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    #endregion

    #region PhoneNumber Validation

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldNotHaveValidationError()
    {
        var request = new UpdateUserRequest
        {
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
        var request = new UpdateUserRequest
        {
            PhoneNumber = phoneNumber
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage(AppConstants.ValidationMessages.PhoneNumberInvalid);
    }

    [Theory]
    [InlineData("+573001234567")]
    [InlineData("+12125551234")]
    [InlineData("+442071234567")]
    public void Validate_WithVariousValidPhoneNumbers_ShouldNotHaveValidationError(string phoneNumber)
    {
        var request = new UpdateUserRequest
        {
            PhoneNumber = phoneNumber
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_WithEmptyPhoneNumber_ShouldNotHaveValidationError()
    {
        var request = new UpdateUserRequest
        {
            PhoneNumber = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    #endregion

    #region NotificationPreference Validation

    [Fact]
    public void Validate_WithInvalidNotificationPreference_ShouldHaveValidationError()
    {
        var request = new UpdateUserRequest
        {
            NotificationPreference = (NotificationType)999
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.NotificationPreference)
            .WithErrorMessage(AppConstants.ValidationMessages.InvalidNotificationPreference);
    }

    [Theory]
    [InlineData(NotificationType.Email)]
    [InlineData(NotificationType.SMS)]
    public void Validate_WithValidNotificationPreference_ShouldNotHaveValidationError(NotificationType preference)
    {
        var request = new UpdateUserRequest
        {
            NotificationPreference = preference
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.NotificationPreference);
    }

    #endregion
}
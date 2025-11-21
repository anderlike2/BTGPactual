using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Domain.Enums;
using BTGPactual.Shared.Constants;
using FluentValidation;

namespace BTGPactual.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage(AppConstants.ValidationMessages.FirstNameMaxLength)
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage(AppConstants.ValidationMessages.LastNameMaxLength)
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+[1-9]\d{1,14}$")
            .WithMessage(AppConstants.ValidationMessages.PhoneNumberInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.NotificationPreference)
            .IsInEnum().WithMessage(AppConstants.ValidationMessages.InvalidNotificationPreference)
            .When(x => x.NotificationPreference.HasValue);
    }
}
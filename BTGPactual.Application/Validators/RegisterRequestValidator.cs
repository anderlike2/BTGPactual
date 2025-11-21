using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Shared.Constants;
using FluentValidation;

namespace BTGPactual.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.UsernameRequired)
            .MinimumLength(3).WithMessage(AppConstants.ValidationMessages.UsernameMinLength)
            .MaximumLength(50).WithMessage(AppConstants.ValidationMessages.UsernameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(AppConstants.ValidationMessages.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.PasswordRequired)
            .MinimumLength(8).WithMessage(AppConstants.ValidationMessages.PasswordMinLength);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.FirstNameRequired)
            .MaximumLength(50).WithMessage(AppConstants.ValidationMessages.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.LastNameRequired)
            .MaximumLength(50).WithMessage(AppConstants.ValidationMessages.LastNameMaxLength);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+[1-9]\d{1,14}$")
            .WithMessage(AppConstants.ValidationMessages.PhoneNumberInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.NotificationPreference)
            .IsInEnum().WithMessage(AppConstants.ValidationMessages.InvalidNotificationPreference);
    }
}
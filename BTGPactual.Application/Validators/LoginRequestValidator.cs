using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Extensions;
using FluentValidation;

namespace BTGPactual.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.EmailRequired)
            .Must(email => email.IsValidEmail()).WithMessage(AppConstants.ValidationMessages.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.PasswordRequired)
            .MinimumLength(8).WithMessage(AppConstants.ValidationMessages.PasswordMinLength);
    }
}
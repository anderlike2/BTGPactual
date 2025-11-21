using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Shared.Constants;
using FluentValidation;

namespace BTGPactual.Application.Validators;

public class CreateFundRequestValidator : AbstractValidator<CreateFundRequest>
{
    public CreateFundRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.FundNameRequired)
            .MaximumLength(100).WithMessage(AppConstants.ValidationMessages.FundNameMaxLength);

        RuleFor(x => x.MinimumAmount)
            .GreaterThan(0).WithMessage(AppConstants.ValidationMessages.MinimumAmountMustBeGreaterThanZero);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(AppConstants.ValidationMessages.InvalidFundCategory);
    }
}
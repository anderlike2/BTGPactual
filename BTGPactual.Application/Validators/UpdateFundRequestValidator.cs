using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Shared.Constants;
using FluentValidation;

namespace BTGPactual.Application.Validators;

public class UpdateFundRequestValidator : AbstractValidator<UpdateFundRequest>
{
    public UpdateFundRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage(AppConstants.ValidationMessages.FundNameMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.MinimumAmount)
            .GreaterThan(0).WithMessage(AppConstants.ValidationMessages.MinimumAmountMustBeGreaterThanZero)
            .When(x => x.MinimumAmount.HasValue);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(AppConstants.ValidationMessages.InvalidFundCategory)
            .When(x => x.Category.HasValue);
    }
}
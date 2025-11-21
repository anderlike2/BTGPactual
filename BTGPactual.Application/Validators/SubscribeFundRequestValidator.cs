using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Extensions;
using FluentValidation;

namespace BTGPactual.Application.Validators;

public class SubscribeFundRequestValidator : AbstractValidator<SubscribeFundRequest>
{
    public SubscribeFundRequestValidator()
    {
        RuleFor(x => x.FundId)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.FundIdRequired)
            .Must(id => id.IsValidObjectId()).WithMessage(AppConstants.ValidationMessages.InvalidObjectId);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(AppConstants.ValidationMessages.AmountMustBeGreaterThanZero);
    }
}
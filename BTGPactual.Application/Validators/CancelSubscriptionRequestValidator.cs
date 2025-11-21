using BTGPactual.Application.DTOs.Requests;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Extensions;
using FluentValidation;

namespace BTGPactual.Application.Validators;

public class CancelSubscriptionRequestValidator : AbstractValidator<CancelSubscriptionRequest>
{
    public CancelSubscriptionRequestValidator()
    {
        RuleFor(x => x.FundId)
            .NotEmpty().WithMessage(AppConstants.ValidationMessages.FundIdRequired)
            .Must(id => id.IsValidObjectId()).WithMessage(AppConstants.ValidationMessages.InvalidObjectId);
    }
}
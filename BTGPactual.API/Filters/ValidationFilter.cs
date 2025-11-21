using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTGPactual.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            List<string> errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            ApiResponse<object> response = ApiResponse<object>.ErrorResponse(
                AppConstants.ErrorMessages.ValidationError, 
                errors);

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}
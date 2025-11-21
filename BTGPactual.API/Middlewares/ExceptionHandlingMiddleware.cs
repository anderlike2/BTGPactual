using System.Net;
using System.Text.Json;
using BTGPactual.Application.Exceptions;
using BTGPactual.Domain.Exceptions;
using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using FluentValidation;

namespace BTGPactual.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ApiResponse<object> response = exception switch
        {
            ValidationException validationException => HandleValidationException(context, validationException),
            NotFoundException notFoundException => HandleNotFoundException(context, notFoundException),
            BusinessException businessException => HandleBusinessException(context, businessException),
            DomainException domainException => HandleDomainException(context, domainException),
            _ => HandleUnknownException(context, exception)
        };

        _logger.LogError(exception, "Error en la aplicación: {Message}", exception.Message);

        string result = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(result);
    }

    private static ApiResponse<object> HandleValidationException(HttpContext context, ValidationException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        List<string> errors = exception.Errors
            .Select(e => e.ErrorMessage)
            .ToList();

        return ApiResponse<object>.ErrorResponse(AppConstants.ErrorMessages.ValidationError, errors);
    }

    private static ApiResponse<object> HandleNotFoundException(HttpContext context, NotFoundException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        return ApiResponse<object>.ErrorResponse(exception.Message);
    }

    private static ApiResponse<object> HandleBusinessException(HttpContext context, BusinessException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return ApiResponse<object>.ErrorResponse(exception.Message);
    }

    private static ApiResponse<object> HandleDomainException(HttpContext context, DomainException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return ApiResponse<object>.ErrorResponse(exception.Message);
    }

    private static ApiResponse<object> HandleUnknownException(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return ApiResponse<object>.ErrorResponse(AppConstants.ErrorMessages.InternalServerError);
    }
}
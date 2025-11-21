using System.Reflection;
using BTGPactual.Application.Services.Implementations;
using BTGPactual.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BTGPactual.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFundService, FundService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
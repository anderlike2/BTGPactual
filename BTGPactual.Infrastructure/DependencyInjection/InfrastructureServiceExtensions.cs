using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleNotificationService;
using Amazon.Extensions.NETCore.Setup;
using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Interfaces;
using BTGPactual.Infrastructure.Data.Context;
using BTGPactual.Infrastructure.Data.Repositories;
using BTGPactual.Infrastructure.Data.Seeds;
using BTGPactual.Infrastructure.ExternalServices.Email;
using BTGPactual.Infrastructure.ExternalServices.SMS;
using BTGPactual.Infrastructure.Identity;
using BTGPactual.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BTGPactual.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDbSettings"));

        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFundRepository, FundRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        AwsSettings? awsSettings = configuration.GetSection("AwsSettings").Get<AwsSettings>();
        services.Configure<AwsSettings>(
            configuration.GetSection("AwsSettings"));

        bool useRealAws = configuration.GetValue<bool>("UseRealAwsServices", false);

        if (useRealAws && awsSettings != null)
        {
            RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Region);

            AWSOptions awsOptions = configuration.GetAWSOptions();
            awsOptions.Region = regionEndpoint;

            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonSimpleEmailService>();
            services.AddAWSService<IAmazonSimpleNotificationService>();

            services.AddScoped<IEmailService, AwsSesEmailService>();
            //services.AddScoped<ISmsService, AwsSnsService>();
            services.AddScoped<ISmsService, FakeSmsService>();
        }
        else
        {
            services.AddScoped<IEmailService, FakeEmailService>();
            services.AddScoped<ISmsService, FakeSmsService>();
        }

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
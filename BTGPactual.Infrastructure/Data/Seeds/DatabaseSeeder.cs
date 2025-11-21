using BTGPactual.Application.Services.Interfaces;
using BTGPactual.Domain.Entities;
using BTGPactual.Domain.Enums;
using BTGPactual.Infrastructure.Data.Context;
using BTGPactual.Shared.Constants;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BTGPactual.Infrastructure.Data.Seeds;

public class DatabaseSeeder
{
    private readonly MongoDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        MongoDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedFundsAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedFundsAsync()
    {
        IMongoCollection<Fund> fundsCollection = _context.GetCollection<Fund>(MongoCollections.Funds);

        long existingFunds = await fundsCollection.CountDocumentsAsync(_ => true);
        if (existingFunds > 0)
        {
            _logger.LogInformation("Los fondos ya existen en la base de datos");
            return;
        }

        List<Fund> funds = new List<Fund>
        {
            new Fund
            {
                Name = "FPV_BTG_PACTUAL_RECAUDADORA",
                MinimumAmount = 75000m,
                Category = FundCategory.FPV,
                IsActive = true,
                Description = "Fondo de pensiones voluntarias BTG Pactual Recaudadora"
            },
            new Fund
            {
                Name = "FPV_BTG_PACTUAL_ECOPETROL",
                MinimumAmount = 125000m,
                Category = FundCategory.FPV,
                IsActive = true,
                Description = "Fondo de pensiones voluntarias BTG Pactual Ecopetrol"
            },
            new Fund
            {
                Name = "DEUDAPRIVADA",
                MinimumAmount = 50000m,
                Category = FundCategory.FIC,
                IsActive = true,
                Description = "Fondo de inversión colectiva en deuda privada"
            },
            new Fund
            {
                Name = "FDO-ACCIONES",
                MinimumAmount = 250000m,
                Category = FundCategory.FIC,
                IsActive = true,
                Description = "Fondo de inversión colectiva en acciones"
            },
            new Fund
            {
                Name = "FPV_BTG_PACTUAL_DINAMICA",
                MinimumAmount = 100000m,
                Category = FundCategory.FPV,
                IsActive = true,
                Description = "Fondo de pensiones voluntarias BTG Pactual Dinámica"
            }
        };

        await fundsCollection.InsertManyAsync(funds);
        _logger.LogInformation("Seed de {FundCount} fondos completado exitosamente", funds.Count);
    }

    private async Task SeedAdminUserAsync()
    {
        IMongoCollection<User> usersCollection = _context.GetCollection<User>(MongoCollections.Users);

        User existingAdmin = await usersCollection.Find(u => u.Role == UserRole.Admin).FirstOrDefaultAsync();
        if (existingAdmin != null)
        {
            _logger.LogInformation("Usuario administrador ya existe");
            return;
        }

        User adminUser = new User
        {
            Username = "admin",
            Email = AppConstants.DefaultValues.AdminEmail,
            PasswordHash = _passwordHasher.HashPassword(AppConstants.DefaultValues.AdminPassword),
            Role = UserRole.Admin,
            Balance = 0,
            NotificationPreference = NotificationType.Email,
            IsActive = true
        };

        await usersCollection.InsertOneAsync(adminUser);

        _logger.LogInformation(
            "Usuario administrador creado - Email: {Email}, Password: {Password}",
            AppConstants.DefaultValues.AdminEmail, 
            AppConstants.DefaultValues.AdminPassword);
    }
}
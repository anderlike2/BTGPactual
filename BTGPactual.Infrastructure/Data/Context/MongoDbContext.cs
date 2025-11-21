using BTGPactual.Shared.Constants;
using BTGPactual.Shared.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BTGPactual.Infrastructure.Data.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        MongoDbSettings mongoSettings = settings.Value;
        MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
        clientSettings.RetryWrites = false;
        clientSettings.SslSettings = new SslSettings
        {
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
            CheckCertificateRevocation = false
        };
        MongoClient client = new MongoClient(clientSettings);
        _database = client.GetDatabase(mongoSettings.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public IMongoCollection<T> Users<T>() => GetCollection<T>(MongoCollections.Users);
    public IMongoCollection<T> Funds<T>() => GetCollection<T>(MongoCollections.Funds);
    public IMongoCollection<T> Transactions<T>() => GetCollection<T>(MongoCollections.Transactions);
}
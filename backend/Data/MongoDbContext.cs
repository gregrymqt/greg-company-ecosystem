using MongoDB.Driver;

namespace MeuCrudCsharp.Data;

public interface IMongoDbContext
{
    IMongoDatabase Database { get; }
    IMongoCollection<T> GetCollection<T>(string name);
    Task<IClientSessionHandle> StartSessionAsync();
}

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoClient _client;
    public IMongoDatabase Database { get; }

    public MongoDbContext(IMongoClient client, MongoDbSettings settings)
    {
        _client = client;

        // Configura o nível de consistência do banco com base no .env
        var dbSettings = new MongoDatabaseSettings
        {
            WriteConcern = settings.WriteConcern.Equals("Majority", StringComparison.OrdinalIgnoreCase)
                ? WriteConcern.WMajority
                : WriteConcern.Acknowledged
        };

        // Aplica a garantia de gravação no Log de Transações (Journal) se ativo
        if (settings.Journal)
        {
            dbSettings.WriteConcern = dbSettings.WriteConcern.With(journal: true);
        }

        Database = _client.GetDatabase(settings.DatabaseName, dbSettings);
    }

    public IMongoCollection<T> GetCollection<T>(string name) 
        => Database.GetCollection<T>(name);

    public async Task<IClientSessionHandle> StartSessionAsync()
    {
        var options = new ClientSessionOptions
        {
            DefaultTransactionOptions = new TransactionOptions(
                readConcern: ReadConcern.Snapshot, // Crucial para consistência em transações ACID
                writeConcern: WriteConcern.WMajority,
                maxCommitTime: TimeSpan.FromSeconds(10))
        };
        
        return await _client.StartSessionAsync(options);
    }
}
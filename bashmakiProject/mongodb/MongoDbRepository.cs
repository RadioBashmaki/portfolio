using MongoDB.Driver;

namespace bashmakiProject.mongodb;

public class MongoDbRepository : IMongoDbRepository
{
    public readonly IMongoDatabase Database;

    public MongoDbRepository(IMongoClient client, string dbName)
    {
        Database = client.GetDatabase(dbName);
    }

    public IMongoCollection<T> GetCollection<T>() where T : class
    {
        return Database.GetCollection<T>(GetCollectionName<T>());
    }
    
    private static string? GetCollectionName<T>() where T : class
    {
        return (typeof(T).GetCustomAttributes(typeof(MongoCollectionAttribute), true).FirstOrDefault() as
            MongoCollectionAttribute)?.CollectionName;
    }
}
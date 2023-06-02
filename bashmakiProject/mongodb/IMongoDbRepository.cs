using MongoDB.Driver;

namespace bashmakiProject.mongodb;

public interface IMongoDbRepository
{
    public IMongoCollection<T> GetCollection<T>() where T : class;
}
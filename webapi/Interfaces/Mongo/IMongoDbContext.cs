using MongoDB.Driver;

namespace webapi.Interfaces.Mongo
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}

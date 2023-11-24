using MongoDB.Driver;
using webapi.Interfaces.Mongo;

namespace webapi.DB.MongoDb
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IConfiguration _configuration;

        public MongoDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var client = new MongoClient(_configuration.GetConnectionString("MongoDbConnection")!);
            _database = client.GetDatabase("FileCrypt");
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}

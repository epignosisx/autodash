using MongoDB.Driver;

namespace Autodash.Core
{
    public class MongoDatabaseProvider
    {
        public static IMongoDatabase GetDatabase()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("Autodash");
            
            return db;
        }
    }

}

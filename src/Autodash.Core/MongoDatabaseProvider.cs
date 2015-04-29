using FluentValidation.Results;
using MongoDB.Driver;
using System.Text;

namespace Autodash.Core
{
    public class MongoDatabaseProvider
    {
        public IMongoDatabase GetDatabase()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("Autodash");
            
            return db;
        }
    }

}

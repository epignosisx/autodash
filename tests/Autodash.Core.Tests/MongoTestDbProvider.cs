using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core.Tests
{
    public static class MongoTestDbProvider
    {
        private static bool _init = false;
        private static string _databaseName = "Autodash_Test";
        private static AsyncLock _asyncLock = new AsyncLock();

        public static async Task<IMongoDatabase> GetDatabase()
        {
            await EnsureInitialized();
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase(_databaseName);
            return db;
        }

        private static async Task EnsureInitialized()
        {
            if (_init)
                return;


            using (await _asyncLock.LockAsync())
            {
                if (_init)
                    return;

                var client = new MongoClient("mongodb://localhost:27017");
                await client.DropDatabaseAsync(_databaseName);
                _init = true;

            }
        }
    }
}

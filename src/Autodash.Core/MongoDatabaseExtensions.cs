﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Autodash.Core
{
    public static class MongoDatabaseExtensions
    {
        public static async Task<TestSuite> GetSuiteByIdAsync(this IMongoDatabase database, string id)
        {
            var queryById = Builders<TestSuite>.Filter.Eq(n => n.Id, id);
            var results = await database.GetCollection<TestSuite>("TestSuite").FindAsync(queryById);
            await results.MoveNextAsync();
            TestSuite suite = results.Current.FirstOrDefault();
            return suite;
        }

        public static async Task<List<SuiteRun>> GetSuiteRunsBySuiteIdAsync(this IMongoDatabase database, string suiteId, int take = -1)
        {
            var query = Builders<SuiteRun>.Filter.Eq(n => n.TestSuiteId, suiteId);
            var opts = new FindOptions<SuiteRun>();
            opts.Sort = Builders<SuiteRun>.Sort.Descending(n => n.ScheduledFor);

            if (take > 0)
                opts.Limit = take;

            var results = await database.GetCollection<SuiteRun>("SuiteRun").FindAsync(query, opts).ToListAsync();
            return results;
        }

        public static async Task<List<SuiteRun>> GetSuiteRunsByProjectIdAsync(this IMongoDatabase database,
            string projectId, int take = -1)
        {
            var query = Builders<SuiteRun>.Filter.Eq(n => n.TestSuiteSnapshot.ProjectId, projectId);
            var opts = new FindOptions<SuiteRun>();
            opts.Sort = Builders<SuiteRun>.Sort.Descending(n => n.ScheduledFor);

            if (take > 0)
                opts.Limit = take;

            var results = await database.GetCollection<SuiteRun>("SuiteRun").FindAsync(query, opts).ToListAsync();
            return results;
        }

        public static async Task<List<SuiteRun>> GetSuiteRunsBySuiteIdsAsync(this IMongoDatabase database,
            IEnumerable<string> suiteIds, int take)
        {
            
            List<FilterDefinition<SuiteRun>> conditions = new List<FilterDefinition<SuiteRun>>();

            foreach (var suiteId in suiteIds)
            {
                FilterDefinition<SuiteRun> d = Builders<SuiteRun>.Filter.Eq(n => n.TestSuiteId, suiteId);
                conditions.Add(d);
            }

            if (conditions.Count == 0)
                return await Task.FromResult(new List<SuiteRun>(0));

            var query = Builders<SuiteRun>.Filter.Or(conditions);

            var opts = new FindOptions<SuiteRun>();
            opts.Sort = Builders<SuiteRun>.Sort.Descending(n => n.ScheduledFor);
            opts.Limit = take;

            var results = await database.GetCollection<SuiteRun>("SuiteRun").FindAsync(query, opts).ToListAsync();
            return results;
        }

        public static async Task<Project> GetProjectByIdAsync(this IMongoDatabase database, string projectId)
        {
            var queryBuilder = Builders<Project>.Filter;
            var filterById = queryBuilder.Eq(n => n.Id, projectId);

            var results = await database.GetCollection<Project>("Project").FindAsync(filterById);
            Project project = null;
            while (await results.MoveNextAsync())
            {
                project = results.Current.First();
                break;
            }
            return project;
        }

        public static async Task<List<TestSuite>> GetSuitesByProjectIdAsync(this IMongoDatabase database, string projectId)
        {
            var queryBuilder = Builders<TestSuite>.Filter;
            var filter = queryBuilder.Eq(n => n.ProjectId, projectId);

            var results = await database.GetCollection<TestSuite>("TestSuite").FindAsync(filter).ToListAsync();
            return results;
        }

        public static async Task<SuiteRun> GetSuiteRunByIdAsync(this IMongoDatabase database, string suiteRunId)
        {
            var queryBuilder = Builders<SuiteRun>.Filter;
            var filter = queryBuilder.Eq(n => n.Id, suiteRunId);

            var results = await database.GetCollection<SuiteRun>("SuiteRun").FindAsync(filter).ToListAsync();
            return results.FirstOrDefault();
        }

        public static async Task DeleteProjectAsync(this IMongoDatabase database, string projectId)
        {
            var suiteRunQueryBuilder = Builders<SuiteRun>.Filter;
            var suiteRunFilter = suiteRunQueryBuilder.Eq(n => n.TestSuiteSnapshot.ProjectId, projectId);
            var runResult = await database.GetCollection<SuiteRun>("SuiteRun").DeleteManyAsync(suiteRunFilter);

            var testSuiteQueryBuilder = Builders<TestSuite>.Filter;
            var testSuiteFilter = testSuiteQueryBuilder.Eq(n => n.ProjectId, projectId);
            var suiteResult = await database.GetCollection<TestSuite>("TestSuite").DeleteManyAsync(testSuiteFilter);

            var projectFilter = Builders<Project>.Filter.Eq(n => n.Id, projectId);
            var projectResult = await database.GetCollection<Project>("Project").DeleteOneAsync(projectFilter);
        }

        public static async Task DeleteTestSuiteAsync(this IMongoDatabase database, string testSuiteId)
        {
            var suiteRunQueryBuilder = Builders<SuiteRun>.Filter;
            var suiteRunFilter = suiteRunQueryBuilder.Eq(n => n.TestSuiteId, testSuiteId);
            var runResult = await database.GetCollection<SuiteRun>("SuiteRun").DeleteManyAsync(suiteRunFilter);

            var testSuiteQueryBuilder = Builders<TestSuite>.Filter;
            var testSuiteFilter = testSuiteQueryBuilder.Eq(n => n.Id, testSuiteId);
            var suiteResult = await database.GetCollection<TestSuite>("TestSuite").DeleteOneAsync(testSuiteFilter);
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<IAsyncCursor<T>> cursorTask)
        {
            var cursor = await cursorTask;
            List<T> results = new List<T>();
            while (await cursor.MoveNextAsync())
            {
                results.AddRange(cursor.Current);
            }

            return results;
        }

        public static async Task<T> ToFirstOrDefaultAsync<T>(this Task<IAsyncCursor<T>> cursorTask)
        {
            var results = await cursorTask.ToListAsync();
            return results.FirstOrDefault();
        }
    }
}

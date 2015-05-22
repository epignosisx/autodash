using System.Linq.Dynamic;
using Autodash.Core.UI.Models;
using FluentValidation.Results;
using MongoDB.Driver;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.ModelBinding;
using Nancy.TinyIoc;
using FluentValidation;
using System.IO;

namespace Autodash.Core.UI.Modules
{
    public class SuitesModule : NancyModule
    {
        public SuitesModule(TinyIoCContainer container)
        {
            Get["/projects/{id}/suites/create"] = parameters => {
                var vm = new CreateSuiteVm
                {
                    ProjectId = parameters.id
                };

                return View["CreateSuite", vm];
            };

            Post["/suites/create", true] = async (c, ct) => {
                var vm = this.Bind<CreateSuiteVm>();
                var zip = Request.Files.FirstOrDefault();

                if (zip == null)
                {
                    vm.Errors = new [] { new ValidationFailure("TestAssembliesZip", "Test assemblies zip is required."), };
                    return View["CreateSuite", vm];              
                }

                var tempZipLoc = Path.GetTempFileName();
                using(var fs = new FileStream(tempZipLoc, FileMode.Append))
                {
                    await zip.Value.CopyToAsync(fs);
                }

                var cmd = container.Resolve<CreateSuiteCommand>();

                var suite = new TestSuite { 
                    ProjectId = vm.ProjectId,
                    Name = vm.SuiteName,
                    Configuration = new TestSuiteConfiguration
                    {
                        Browsers = vm.Browsers,
                        RetryAttempts = vm.RetryAttempts,
                        TestTimeout = TimeSpan.FromMinutes(vm.TestTimeoutMinutes)
                    },
                    Schedule = vm.Time.HasValue ? new TestSuiteSchedule {
                        Time = vm.Time.Value,
                        Interval = vm.IntervalHours.HasValue ? 
                            TimeSpan.FromHours(vm.IntervalHours.Value) : 
                            TimeSpan.MinValue
                    } : null
                };

                try
                {
                    await cmd.ExecuteAsync(suite, tempZipLoc);
                }
                catch (ValidationException ex)
                {
                    vm.Errors = ex.Errors.ToArray();
                    return View["CreateSuite", vm];
                }
                finally
                {
                    File.Delete(tempZipLoc);
                }

                return Response.AsRedirect(string.Format("/suites/{0}", suite.Id));
            };

            Post["/suites/update", true] = async (parameters, ct) =>
            {
                var vm = this.Bind<EditSuiteVm>();
                var database = container.Resolve<IMongoDatabase>();
                var existingSuite = await GetSuiteById(database, vm.Id);

                var suite = new TestSuite
                {
                    Id = vm.Id,
                    ProjectId = vm.ProjectId,
                    Name = vm.SuiteName,
                    Configuration = new TestSuiteConfiguration
                    {
                        Browsers = vm.Browsers,
                        RetryAttempts = vm.RetryAttempts,
                        TestTimeout = TimeSpan.FromMinutes(vm.TestTimeoutMinutes),
                        TestAssembliesPath = existingSuite.Configuration.TestAssembliesPath
                    },
                    Schedule = vm.Time.HasValue ? new TestSuiteSchedule
                    {
                        Time = vm.Time.Value,
                        Interval = vm.IntervalHours.HasValue ?
                            TimeSpan.FromHours(vm.IntervalHours.Value) :
                            TimeSpan.MinValue
                    } : null
                };

                var queryById = Builders<TestSuite>.Filter.Eq(n => n.Id, suite.Id);
                await database.GetCollection<TestSuite>("TestSuite").FindOneAndReplaceAsync(queryById, suite);
                return Response.AsRedirect("/suites/" + suite.Id);
            };

            Get["/suites/{id}", true] = async (parameters, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                TestSuite suite = await GetSuiteById(database, parameters.id);

                if (suite == null)
                {
                    var response = Response.AsText("Not Found");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var unitTestDiscoverer = container.Resolve<ITestSuiteUnitTestDiscoverer>();
                UnitTestCollection[] unitTestCollections = unitTestDiscoverer.Discover(suite.Configuration.TestAssembliesPath).ToArray();

                var vm = new SuiteDetailsVm
                {
                    Suite = suite,
                    UnitTestCollections = unitTestCollections
                };

                return View["SuiteDetails", vm];
            };
        }

        private static async Task<TestSuite> GetSuiteById(IMongoDatabase database, string id)
        {
            var queryById = Builders<TestSuite>.Filter.Eq(n => n.Id, id);
            var results = await database.GetCollection<TestSuite>("TestSuite").FindAsync(queryById);
            await results.MoveNextAsync();
            TestSuite suite = results.Current.FirstOrDefault();
            return suite;
        }
    }
}

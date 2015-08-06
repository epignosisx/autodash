using System.Globalization;
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
                var availableBrowserProvider = container.Resolve<ISeleniumGridBrowserProvider>();

                var vm = new CreateSuiteVm
                {
                    ProjectId = parameters.id,
                    AvailableBrowsers = GetAvailableBrowsers(availableBrowserProvider).ToList()
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
                        EnvironmentUrl = vm.EnvironmentUrl,
                        Browsers = vm.GetBrowsers().ToArray(),
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
                var existingSuite = await database.GetSuiteByIdAsync(vm.Id);

                var suite = new TestSuite
                {
                    Id = vm.Id,
                    ProjectId = vm.ProjectId,
                    Name = vm.SuiteName,
                    Configuration = new TestSuiteConfiguration
                    {
                        EnvironmentUrl = vm.EnvironmentUrl,
                        Browsers = vm.GetBrowsers().ToArray(),
                        RetryAttempts = vm.RetryAttempts,
                        TestTimeout = TimeSpan.FromMinutes(vm.TestTimeoutMinutes),
                        TestAssembliesPath = existingSuite.Configuration.TestAssembliesPath,
                        SelectedTests = existingSuite.Configuration.SelectedTests
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

            Get["/suites/{id}/tests", true] = async (parameters, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                string id = parameters.id;
                TestSuite suite = await database.GetSuiteByIdAsync(id);
                return Response.AsJson(new { Tests = suite.Configuration.SelectedTests ?? new string[0] });
            };

            Post["/suites/tests/update", true] = async (parameters, ct) =>
            {
                var vm = this.Bind<UpdateSuiteTestsVm>();
                var database = container.Resolve<IMongoDatabase>();
                var existingSuite = await database.GetSuiteByIdAsync(vm.Id);
                existingSuite.Configuration.SelectedTests = vm.Tests.ToArray();
                var queryById = Builders<TestSuite>.Filter.Eq(n => n.Id, vm.Id);
                await database.GetCollection<TestSuite>("TestSuite").FindOneAndReplaceAsync(queryById, existingSuite);
                return Response.AsJson(new { Success = true });
            };

            Post["/suites/delete", true] = async (x, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                string id = Request.Form.id;
                await database.DeleteTestSuiteAsync(id);
                return Response.AsJson(new {Success = true});
            };

            Get["/suites/{id}", true] = async (parameters, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                string id = parameters.id;
                TestSuite suite = await database.GetSuiteByIdAsync(id);

                if (suite == null)
                {
                    var response = Response.AsText("Not Found");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var suiteRuns = await database.GetSuiteRunsBySuiteIdAsync(id);

                var fileExplorer = container.Resolve<ITestSuiteFileExplorer>();;
                var files = fileExplorer.GetFiles(suite.Configuration.TestAssembliesPath)
                                        .Select(n => new FileExplorerItem
                                        {
                                            Filename = n,
                                            EditLink = string.Format("/suites/{0}/file-editor?file={1}", Uri.EscapeDataString(suite.Id), Uri.EscapeDataString(n))
                                        }).ToList();

                var availableBrowserProvider = container.Resolve<ISeleniumGridBrowserProvider>();
                
                var vm = new SuiteDetailsVm
                {
                    Suite = suite,
                    SuiteRuns = suiteRuns,
                    FileExplorer = new FileExplorerVm
                    {
                        TestSuiteId = suite.Id,
                        Files = files
                    },
                    AvailableBrowsers = GetAvailableBrowsers(availableBrowserProvider).ToList()
                };

                return View["SuiteDetails", vm];
            };

            Get["/suites/{id}/test-tree-map", true] = async (x, ct) =>
            {
                string id = x.id;
                var database = container.Resolve<IMongoDatabase>();
                TestSuite suite = await database.GetSuiteByIdAsync(id);
                if (suite == null)
                    return Response.AsJson(new { Error = "Suite not found" }, HttpStatusCode.NotFound);

                var unitTestDiscoverer = container.Resolve<ITestSuiteUnitTestDiscoverer>();
                UnitTestCollection[] unitTestCollections = unitTestDiscoverer.Discover(suite.Configuration.TestAssembliesPath).ToArray();

                var tagCount = new Dictionary<string, int>();
                
                foreach (var test in unitTestCollections.SelectMany(n => n.Tests))
                {
                    foreach (var tag in test.TestTags)
                    {
                        if (tagCount.ContainsKey(tag))
                        {
                            tagCount[tag]++;
                        }
                        else
                        {
                            tagCount.Add(tag, 1);
                        }
                    }
                }

                var mostUsedTag = (double)tagCount.OrderByDescending(n => n.Value).First().Value;
                var data = tagCount.OrderBy(n => n.Key)
                                   .Select(n => new
                                   {
                                       TagName = n.Key, 
                                       Count = n.Value, 
                                       Percentage = 100*(n.Value/mostUsedTag)
                                   });
                return Response.AsJson(new { Tags =  data});
            };

            Get["/suites/{id}/test-explorer", true] = async (parameters, ct) =>
            {
                string id = parameters.id;
                var database = container.Resolve<IMongoDatabase>();
                TestSuite suite = await database.GetSuiteByIdAsync(id);
                string query = Request.Query.query;
                if (suite == null)
                    return Response.AsJson(new { Error = "Suite not found" }, HttpStatusCode.NotFound);

                var unitTestDiscoverer = container.Resolve<ITestSuiteUnitTestDiscoverer>();
                UnitTestCollection[] unitTestCollections = unitTestDiscoverer.Discover(suite.Configuration.TestAssembliesPath).ToArray();

                var vm = new TestExplorerVm
                {
                    UnitTestCollections = new List<UnitTestCollectionVm>(unitTestCollections.Length)
                };

                foreach (var coll in unitTestCollections)
                {
                    var collVm = new UnitTestCollectionVm
                    {
                        AssemblyName = coll.AssemblyName.Split(',')[0],
                        TestRunnerName = coll.Runner.TestRunnerName,
                        Tests = new List<UnitTestInfoVm>(coll.Tests.Length)
                    };

                    foreach (var test in coll.Tests)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(query) || UnitTestTagSelector.Evaluate(query, test.TestTags))
                            {
                                collVm.Tests.Add(new UnitTestInfoVm(
                                    test.TestName, 
                                    test.TestTags, 
                                    suite.Configuration.ContainsTest(test.TestName)
                                ));
                            }
                        }
                        catch
                        {
                            return Response.AsJson(new { Error = "Unable to parse query" }, HttpStatusCode.BadRequest);
                        }
                    }

                    if (collVm.Tests.Count > 0)
                        vm.UnitTestCollections.Add(collVm);
                }

                return Response.AsJson(vm);
            };

            Get["/suites/{id}/suite-run-history", true] = async (parameters, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                string id = parameters.id;
                var suiteRuns = await database.GetSuiteRunsBySuiteIdAsync(id);
                if (Request.Query.format == "json")
                    return Response.AsJson(new { SuiteRuns = suiteRuns });

                return View["_SuiteRunHistory", suiteRuns];
            };

            Post["/suites/{id}/schedule", true] = async (parameters, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                var scheduler = container.Resolve<ISuiteRunScheduler>();
                string id = parameters.id;
                TestSuite suite = await database.GetSuiteByIdAsync(id);
                SuiteRun run = await scheduler.Schedule(suite);
                return Response.AsJson(new { SuiteRunId = run.Id });
            };

            Get["/suites/{id}/file-editor", true] = async (parameters, ct) =>
            {
                var database = container.Resolve<IMongoDatabase>();
                string id = parameters.id;
                string fileName = Request.Query.file;
                var suite = await database.GetSuiteByIdAsync(id);

                var fileExplorer = container.Resolve<ITestSuiteFileExplorer>();
                var content = fileExplorer.GetFileContent(suite.Configuration.TestAssembliesPath, fileName);

                var vm = new FileExplorerItem
                {
                    TestSuiteId = id,
                    Filename = fileName,
                    FileContent = content,
                };

                return View["FileEditor", vm];
            };

            Post["/suites/file-editor", true] = async (parameters, ct) =>
            {
                FileExplorerItem vm = this.Bind<FileExplorerItem>();

                var database = container.Resolve<IMongoDatabase>();
                var suite = await database.GetSuiteByIdAsync(vm.TestSuiteId);

                var fileExplorer = container.Resolve<ITestSuiteFileExplorer>();
                fileExplorer.UpdateFileContent(
                    suite.Configuration.TestAssembliesPath, 
                    vm.Filename,
                    vm.FileContent
                );

                return Response.AsRedirect("/suites/" + Uri.EscapeDataString(vm.TestSuiteId));
            };
        }

        private static IEnumerable<KeyValuePair<string, string>> GetAvailableBrowsers(ISeleniumGridBrowserProvider browserProvider)
        {
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            foreach (var browser in browserProvider.GetBrowsers())
            {
                yield return new KeyValuePair<string, string>(browser.Name + "|" + browser.Version, textInfo.ToTitleCase(browser.ToString()));
            }
        }
    }
}

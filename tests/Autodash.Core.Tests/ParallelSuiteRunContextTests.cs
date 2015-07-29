using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Autodash.Core.Tests
{
    //public class ParallelSuiteRunContextTests
    //{
    //    private static UnitTestCollection[] GetUnitTestCollections()
    //    {
    //        var test1 = new UnitTestInfo("Test1", null);
    //        var test2 = new UnitTestInfo("Test2", null);
    //        var unitTestCollection = new UnitTestCollection(null, null, new[] { test1, test2 }, null);
    //        return new UnitTestCollection[] { unitTestCollection };
    //    }

    //    private static SuiteRun GetSuiteRun()
    //    {
    //        return new SuiteRun
    //        {
    //            TestSuiteSnapshot = new TestSuite
    //            {
    //                Configuration = new TestSuiteConfiguration {
    //                    Browsers = new[] { "Firefox", "Chrome" },
    //                    RetryAttempts = 2,
    //                    SelectedTests = new []{ "Test1", "Test2" }
    //                }
    //            },
    //            Result = new SuiteRunResult{
    //                CollectionResults = new UnitTestCollectionResult[]{
    //                    new UnitTestCollectionResult{
    //                        UnitTestResults = new List<UnitTestResult>{
    //                            new UnitTestResult { TestName = "Test1" },
    //                            new UnitTestResult { TestName = "Test2" }
    //                        }
    //                    }
    //                }
    //            }
    //        };
    //    }

    //    [Fact]
    //    public void ReturnsFirstTestAvailableToRunForTheNodeBrowser()
    //    {
    //        //arrange
    //        var suiteRun = GetSuiteRun();
    //        var testColls = GetUnitTestCollections();
    //        var gridNodes = new GridNodeInfo[]{
    //            new GridNodeInfo {
    //                MaxSessions = 5,
    //                Browsers = new List<GridNodeBrowserInfo> { new GridNodeBrowserInfo { BrowserName = "Firefox", Protocol = "WebDriver"} }
    //            }
    //        };
    //        var gridNodeManager = new GridNodeManager(gridNodes);
    //        var subject = new ParallelSuiteRunContext(
    //            suiteRun,
    //            CancellationToken.None, 
    //            testColls, 
    //            new TaskCompletionSource<SuiteRun>()
    //        );

    //        //act
    //        var result = subject.FindNextTestToRun(gridNodeManager);

    //        //assert
    //        Assert.NotNull(result);
    //        Assert.Equal(result.Item1, testColls[0]);
    //        Assert.Equal(result.Item2, testColls[0].Tests[0]);
    //    }

    //    [Fact]
    //    public void TestSuiteDoesNotHaveAnyTestsForTheAvailableBrowserNodesReturnsNull()
    //    {
    //        //arrange
    //        var suiteRun = GetSuiteRun();
    //        var testColls = GetUnitTestCollections();
    //        var gridNodes = new GridNodeInfo[]{
    //            new GridNodeInfo {
    //                MaxSessions = 5,
    //                Browsers = new List<GridNodeBrowserInfo> { new GridNodeBrowserInfo { BrowserName = "Crazy Browser", Protocol = "WebDriver"} }
    //            }
    //        };
    //        var gridNodeManager = new GridNodeManager(gridNodes);
    //        var subject = new ParallelSuiteRunContext(
    //            suiteRun,
    //            CancellationToken.None,
    //            testColls,
    //            new TaskCompletionSource<SuiteRun>()
    //        );

    //        //act
    //        var result = subject.FindNextTestToRun(gridNodeManager);

    //        //assert
    //        Assert.Null(result);
    //    }
    //}
}

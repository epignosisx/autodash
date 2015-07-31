using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodash.Core.MsTest;

namespace Autodash.Core
{
    //public class MsTestRunner : IUnitTestRunner
    //{
    //    private static readonly string CommandTemplate =
    //        "call \"C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\Tools\\VsDevCmd.bat\"" + Environment.NewLine +
    //        "@set \"PATH=C:\\projects\\autodash\\tools\\;%PATH%\"" + Environment.NewLine +
    //        "@set browser={3}" + Environment.NewLine +
    //        "@set environment={4}" + Environment.NewLine +
    //        "mstest.exe /testcontainer:{0} /test:{1} /resultsfile:\"{2}\"";

    //    private static readonly XmlSerializer TestRunSerializer = new XmlSerializer(typeof(TestRun));

    //    public string TestRunnerName { get { return "MSTest Runner"; } }

    //    public async Task<UnitTestResult> Run(
    //        UnitTestInfo unitTest, 
    //        UnitTestCollection testCollection, 
    //        TestSuiteConfiguration config,
    //        CancellationToken cancellationToken)
    //    {
    //        if (unitTest == null)
    //            throw new ArgumentNullException("unitTest");
    //        if (testCollection == null)
    //            throw new ArgumentNullException("testCollection");
    //        if (config == null)
    //            throw new ArgumentNullException("config");

    //        UnitTestResult unitTestResult;
    //        if (config.EnableBrowserExecutionInParallel)
    //            unitTestResult = await RunInParallel(unitTest, testCollection, config, cancellationToken);
    //        else
    //            unitTestResult = RunSerially(unitTest, testCollection, config, cancellationToken);

    //        return unitTestResult;
    //    }

    //    private static UnitTestResult RunSerially(
    //        UnitTestInfo unitTest, 
    //        UnitTestCollection testCollection, 
    //        TestSuiteConfiguration config,
    //        CancellationToken cancellationToken)
    //    {
    //        var browserResults = new List<UnitTestBrowserResult>(config.Browsers.Length);
    //        foreach (var browser in config.Browsers)
    //        {
    //            if (cancellationToken.IsCancellationRequested)
    //                break;

    //            UnitTestBrowserResult browserResult = null;
    //            do
    //            {
    //                int nextAttempt = browserResult == null ? 1 : (browserResult.Attempt + 1);
    //                browserResult = RunTest(unitTest, testCollection, config, browser, nextAttempt);
    //                browserResults.Add(browserResult);
    //            } while (!browserResult.Passed && browserResult.Attempt < config.RetryAttempts && !cancellationToken.IsCancellationRequested);
    //        }

    //        var unitTestResult = new UnitTestResult
    //        {
    //            TestName = unitTest.TestName,
    //            BrowserResults = browserResults
    //        };
    //        return unitTestResult;
    //    }

    //    private static async Task<UnitTestResult> RunInParallel(
    //        UnitTestInfo unitTest, 
    //        UnitTestCollection testCollection,
    //        TestSuiteConfiguration config,
    //        CancellationToken cancellationToken)
    //    {
    //        var ongoingTests = new List<Task<UnitTestBrowserResult>>(config.Browsers.Length);
    //        var browserResults = new List<UnitTestBrowserResult>(ongoingTests.Count);
    //        foreach (var browser in config.Browsers)
    //        {
    //            string browserRef = browser;
    //            ongoingTests.Add(Task.Run(() => RunTest(unitTest, testCollection, config, browserRef, 1)));
    //        }

    //        while (ongoingTests.Count > 0)
    //        {
    //            Task<UnitTestBrowserResult> completed = await Task.WhenAny(ongoingTests.ToArray());
    //            browserResults.Add(completed.Result);
    //            ongoingTests.Remove(completed);

    //            if (!completed.Result.Passed && completed.Result.Attempt < config.RetryAttempts && !cancellationToken.IsCancellationRequested)
    //            {
    //                ongoingTests.Add(Task.Run(() => RunTest(
    //                    unitTest, testCollection, config,
    //                    completed.Result.Browser, completed.Result.Attempt + 1
    //                )));
    //            }
    //        }

    //        var unitTestResult = new UnitTestResult
    //        {
    //            TestName = unitTest.TestName,
    //            BrowserResults = browserResults
    //        };
    //        return unitTestResult;
    //    }

    //    private static UnitTestBrowserResult RunTest(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config, string browser, int attempt)
    //    {
    //        string tempFilename = RemoveInvalidChars(unitTest.ShortTestName + "_" + browser);
    //        string commandFullpath = Path.Combine(config.TestAssembliesPath, tempFilename + ".bat");
    //        string resultFullpath = Path.Combine(config.TestAssembliesPath, tempFilename + ".trx");

    //        string commandContent = string.Format(CommandTemplate,
    //            Path.GetFileName(testCollection.AssemblyPath),
    //            unitTest.TestName,
    //            resultFullpath,
    //            browser,
    //            config.EnvironmentUrl
    //        );

    //        File.WriteAllText(commandFullpath, commandContent);
    //        if (File.Exists(resultFullpath))
    //            File.Delete(resultFullpath);

    //        var info = new ProcessStartInfo();
    //        info.WorkingDirectory = config.TestAssembliesPath;
    //        info.FileName = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), @"System32\cmd.exe");
    //        info.UseShellExecute = false;
    //        info.WindowStyle = ProcessWindowStyle.Hidden;
    //        info.ErrorDialog = false;
    //        info.CreateNoWindow = true;
    //        info.Arguments = "/c \"" + commandFullpath + "\"";
    //        info.RedirectStandardError = true;
    //        info.RedirectStandardOutput = true;

    //        Process process = Process.Start(info);
    //        string stdout = process.StandardOutput.ReadToEnd();
    //        string stderr = process.StandardError.ReadToEnd();

    //        TimeSpan timeout = config.TestTimeout == TimeSpan.Zero ? TimeSpan.FromMinutes(30) : config.TestTimeout;
    //        process.WaitForExit((int)timeout.TotalMilliseconds);

    //        TestRun report;
    //        using (var stream = File.Open(resultFullpath, FileMode.Open))
    //        {
    //            report = (TestRun)TestRunSerializer.Deserialize(stream);
    //        }

    //        //clean up
    //        File.Delete(resultFullpath);
    //        File.Delete(commandFullpath);

    //        bool passed = report.ResultSummary.Counters.passed == "1";

    //        string testOutput = "";
    //        if(report.Results.Length > 0 && report.Results[0].Output != null)
    //            testOutput = report.Results[0].Output.StdOut;

    //        var result = new UnitTestBrowserResult
    //        {
    //            Attempt = attempt,
    //            Browser = browser,
    //            StartTime = report.Results[0].startTime,
    //            EndTime = report.Results[0].endTime,
    //            Stdout = stdout + Environment.NewLine +  "=================" + Environment.NewLine + testOutput,
    //            Stderr = stderr,
    //            Passed = passed
    //        };
    //        return result;
    //    }

    //    private static string RemoveInvalidChars(string name)
    //    {
    //        string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
    //        foreach (char c in invalid)
    //        {
    //            name = name.Replace(c.ToString(), "");
    //        }
    //        return name;
    //    }

    //    public Task<UnitTestBrowserResult> Run(TestRunContext context)
    //    {
    //        //make it truly async by listening to the Process.Exited event
    //        return Task.Run(() => RunTest(context.UnitTestInfo, context.UnitTestCollection, context.TestSuiteConfiguration, context.GridNodeBrowserInfo.BrowserName, 1));
    //    }
    //}


    public class MsTestRunner : IUnitTestRunner
    {
        private static readonly string CommandTemplate =
            "call \"C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\Tools\\VsDevCmd.bat\"" + Environment.NewLine +
            //"@set \"PATH=C:\\projects\\autodash\\tools\\;%PATH%\"" + Environment.NewLine +
            "mstest.exe /testcontainer:{0} /test:{1} /resultsfile:\"{2}\"";

        private static readonly XmlSerializer TestRunSerializer = new XmlSerializer(typeof(TestRun));

        public string TestRunnerName {
            get { return "MSTest Runner"; }
        }

        public Task<UnitTestBrowserResult> Run(TestRunContext context)
        {
            return RunTest(context.UnitTestInfo, context.UnitTestCollection, context.TestSuiteConfiguration, context.GridNodeBrowserInfo.BrowserName);
        }

        private static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest);
            }

            //foreach (string folder in Directory.GetDirectories(sourcePath))
            //{
            //    string dest = Path.Combine(destPath, Path.GetFileName(folder));
            //    CopyDirectory(folder, dest);
            //}
        }

        private static Task<UnitTestBrowserResult> RunTest(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config, string browser)
        {
            string tempFilename = RemoveInvalidChars(unitTest.ShortTestName + "_" + browser);
            string testDir = Path.Combine(config.TestAssembliesPath, tempFilename);
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, true);

            CopyDirectory(config.TestAssembliesPath, testDir);

            string commandFullpath = Path.Combine(testDir, "command.bat");
            string resultFullpath = Path.Combine(testDir, "report.trx");

            string commandContent = string.Format(CommandTemplate,
                Path.GetFileName(testCollection.AssemblyPath),
                unitTest.TestName,
                resultFullpath,
                browser,
                config.EnvironmentUrl
            );

            File.WriteAllText(commandFullpath, commandContent);

            var info = new ProcessStartInfo();
            info.WorkingDirectory = config.TestAssembliesPath;
            info.FileName = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), @"System32\cmd.exe");
            info.UseShellExecute = false;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.ErrorDialog = false;
            info.CreateNoWindow = true;
            info.Arguments = "/c \"" + commandFullpath + "\"";
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;

            Process process = new Process();
            process.StartInfo = info;
            process.EnableRaisingEvents = true;


            StringBuilder stdout = new StringBuilder();
            DataReceivedEventHandler stdoutHandler = null;
            stdoutHandler = (o, e) => stdout.Append(e.Data);
            process.OutputDataReceived += stdoutHandler;

            StringBuilder stderr = new StringBuilder();
            DataReceivedEventHandler stderrHandler = null;
            stderrHandler = (o, e) => stderr.Append(e.Data);
            process.ErrorDataReceived += stderrHandler;

            TimeSpan timeout = config.TestTimeout == TimeSpan.Zero ? TimeSpan.FromMinutes(30) : config.TestTimeout;

            var completionSource = new TaskCompletionSource<UnitTestBrowserResult>();
            var cancellationToken = new CancellationTokenSource(timeout);
            cancellationToken.Token.Register(() => completionSource.TrySetCanceled(), false);
            EventHandler exitedHandler = null;
            exitedHandler = (o, e) =>
            {
                process.OutputDataReceived -= stdoutHandler;
                process.ErrorDataReceived -= stderrHandler;
                process.Exited -= exitedHandler;
                cancellationToken.Dispose();

                UnitTestBrowserResult result = HandleTestCompletion(browser, resultFullpath, testDir, stdout, stderr);
                completionSource.TrySetResult(result);
            };
            
            process.Exited += exitedHandler;
            process.Start();

            return completionSource.Task;
        }

        private static UnitTestBrowserResult HandleTestCompletion(string browser, string resultFullpath, string testDir, StringBuilder stdout, StringBuilder stderr)
        {
            TestRun report;
            using (var stream = File.Open(resultFullpath, FileMode.Open))
            {
                report = (TestRun) TestRunSerializer.Deserialize(stream);
            }

            //clean up
            Directory.Delete(testDir, true);

            bool passed = report.ResultSummary.Counters.passed == "1";

            string testOutput = "";
            if (report.Results.Length > 0 && report.Results[0].Output != null)
                testOutput = report.Results[0].Output.StdOut;

            var result = new UnitTestBrowserResult
            {
                Browser = browser,
                StartTime = report.Results[0].startTime,
                EndTime = report.Results[0].endTime,
                Stdout = stdout + Environment.NewLine + "=================" + Environment.NewLine + testOutput,
                Stderr = stderr.ToString(),
                Passed = passed
            };
            return result;
        }

        private static string RemoveInvalidChars(string name)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                name = name.Replace(c.ToString(), "");
            }
            return name;
        }
    }
}

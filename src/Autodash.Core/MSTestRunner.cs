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
    public class MsTestRunner : IUnitTestRunner
    {
        private static readonly string CommandTemplate =
            "call \"C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\Tools\\VsDevCmd.bat\"" + Environment.NewLine +
            "set hubUrl={3}" + Environment.NewLine +
            "set browserName={4}" + Environment.NewLine +
            "set browserVersion={5}" + Environment.NewLine +
            "mstest.exe /testcontainer:{0} /test:{1} /resultsfile:\"{2}\"";

        private static readonly XmlSerializer TestRunSerializer = new XmlSerializer(typeof(TestRun));

        public string TestRunnerName {
            get { return "MSTest Runner"; }
        }

        public Task<UnitTestBrowserResult> Run(TestRunContext context)
        {
            UnitTestInfo unitTest = context.UnitTestInfo;
            TestSuiteConfiguration config = context.TestSuiteConfiguration;
            GridNodeBrowserInfo nodeBrowser = context.GridNodeBrowserInfo;
            UnitTestCollection testCollection = context.UnitTestCollection;

            string tempFilename = RemoveInvalidChars(unitTest.ShortTestName + "_" + nodeBrowser.BrowserName);
            string testDir = Path.Combine(config.TestAssembliesPath, tempFilename);
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, true);

            CopyDirectory(config.TestAssembliesPath, testDir);

            string commandFullpath = Path.Combine(testDir, "cmd.bat");
            string resultFullpath = Path.Combine(testDir, "rpt.trx");

            string commandContent = string.Format(CommandTemplate,
                Path.GetFileName(testCollection.AssemblyPath),
                unitTest.TestName,
                resultFullpath,
                context.SeleniumGridConfiguration.RemoteWebDriverUrl,
                context.GridNodeBrowserInfo.BrowserName,
                context.GridNodeBrowserInfo.Version
            );

            File.WriteAllText(commandFullpath, commandContent);

            var preProcessorContext = new TestRunnerPreProcessorContext
            {
                TestDirectory = testDir,
                NodeBrowser = nodeBrowser,
                GridConfiguration = context.SeleniumGridConfiguration,
                TestSuiteConfiguration = config,
                UnitTestInfo = unitTest
            };

            foreach (var preProcessor in TestRunnerPreProcessorProvider.GetAll())
            {
                preProcessor.Process(preProcessorContext);
            }

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

            TimeSpan timeout = config.TestTimeout == TimeSpan.Zero ? TimeSpan.FromMinutes(10) : config.TestTimeout;

            var completionSource = new TaskCompletionSource<UnitTestBrowserResult>();
            var cancellationToken = new CancellationTokenSource(timeout);
            DateTime timeoutStartDate = DateTime.UtcNow;

            cancellationToken.Token.Register(() =>
            {
                var result = new UnitTestBrowserResult
                {
                    Browser = new Browser(nodeBrowser.BrowserName, nodeBrowser.Version),
                    Passed = false,
                    StartTime = timeoutStartDate,
                    EndTime = DateTime.UtcNow,
                    Stdout = "Test timed out"
                };
                completionSource.TrySetResult(result);
            }, false);

            EventHandler exitedHandler = null;
            exitedHandler = (o, e) =>
            {
                process.OutputDataReceived -= stdoutHandler;
                process.ErrorDataReceived -= stderrHandler;
                process.Exited -= exitedHandler;
                cancellationToken.Dispose();

                UnitTestBrowserResult result = HandleTestCompletion(nodeBrowser, resultFullpath, testDir, stdout, stderr);
                completionSource.TrySetResult(result);
            };

            process.Exited += exitedHandler;
            process.Start();

            return completionSource.Task;
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

        private static UnitTestBrowserResult HandleTestCompletion(GridNodeBrowserInfo nodeBrowser, string resultFullpath, string testDir, StringBuilder stdout, StringBuilder stderr)
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
            if (report.Results.Length > 0 && report.Results[0].Output != null && !string.IsNullOrEmpty(report.Results[0].Output.StdOut))
                testOutput = report.Results[0].Output.StdOut;

            string testError = "";
            if (report.Results.Length > 0 && report.Results[0].Output != null && report.Results[0].Output.ErrorInfo != null)
                testError = report.Results[0].Output.ErrorInfo.Message + Environment.NewLine + report.Results[0].Output.ErrorInfo.StackTrace;

            var result = new UnitTestBrowserResult
            {
                Browser = new Browser(nodeBrowser.BrowserName, nodeBrowser.Version),
                StartTime = report.Results[0].startTime,
                EndTime = report.Results[0].endTime,
                Stdout = stdout + Environment.NewLine + "=====MS Test=====" + Environment.NewLine + testOutput,
                Stderr = stderr + Environment.NewLine + "=====MS Test=====" + Environment.NewLine + testError,
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

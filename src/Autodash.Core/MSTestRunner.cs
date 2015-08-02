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
            //"@set \"PATH=C:\\projects\\autodash\\tools\\;%PATH%\"" + Environment.NewLine +
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
                nodeBrowser,
                config.EnvironmentUrl
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
                    Browser = nodeBrowser.BrowserName,
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

                UnitTestBrowserResult result = HandleTestCompletion(nodeBrowser.BrowserName, resultFullpath, testDir, stdout, stderr);
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

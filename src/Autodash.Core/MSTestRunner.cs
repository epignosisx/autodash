using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodash.Core.MsTest;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class MsTestRunner : IUnitTestRunner
    {
        private static string CommandTemplate =
            "call \"C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\Tools\\VsDevCmd.bat\"" + Environment.NewLine +
            "mstest.exe /testcontainer:{0} /test:{1} /resultsfile:\"{2}\"";

        private static readonly XmlSerializer TestRunSerializer = new XmlSerializer(typeof(TestRun));


        public async Task<UnitTestResult> Run(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config)
        {
            if (unitTest == null)
                throw new ArgumentNullException("unitTest");
            if (testCollection == null)
                throw new ArgumentNullException("testCollection");
            if (config == null)
                throw new ArgumentNullException("config");

            List<Task<UnitTestBrowserResult>> ongoingTests = new List<Task<UnitTestBrowserResult>>(config.Browsers.Length);
            List<UnitTestBrowserResult> browserResults = new List<UnitTestBrowserResult>(ongoingTests.Count);
            foreach(var browser in config.Browsers)
            {
                ongoingTests.Add(Task.Run(() => RunTest(unitTest, testCollection, config, browser, 0)));
            }
            
            while (ongoingTests.Count > 0)
            {
                Task<UnitTestBrowserResult> completed = await Task.WhenAny(ongoingTests.ToArray());
                browserResults.Add(completed.Result);
                ongoingTests.Remove(completed);

                if (!completed.Result.Passed && completed.Result.Attempt < config.RetryAttempts)
                {
                    ongoingTests.Add(Task.Run(() => RunTest(
                        unitTest, testCollection, config, 
                        completed.Result.Browser, completed.Result.Attempt + 1
                    )));
                }
            }

            UnitTestResult unitTestResult = new UnitTestResult
            {
                TestName = unitTest.TestName,
                Browsers = browserResults
            };

            return unitTestResult;
        }

        private static UnitTestBrowserResult RunTest(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config, string browser, int attempt)
        {
            string tempFilename = RemoveInvalidChars(unitTest.TestName + "_" + browser);
            string commandFullpath = Path.Combine(config.TestAssembliesPath, tempFilename + ".bat");
            string resultFullpath = Path.Combine(config.TestAssembliesPath, tempFilename + ".trx");

            string commandContent = string.Format(CommandTemplate,
                Path.GetFileName(testCollection.AssemblyPath),
                unitTest.TestName,
                resultFullpath
            );

            File.WriteAllText(commandFullpath, commandContent);
            if (File.Exists(resultFullpath))
                File.Delete(resultFullpath);

            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = config.TestAssembliesPath;
            info.FileName = @"C:\Windows\System32\cmd.exe";
            info.UseShellExecute = false;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.ErrorDialog = false;
            info.CreateNoWindow = false;
            info.Arguments = "/c \"" + commandFullpath + "\"";
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;

            Process process = Process.Start(info);
            string stderr = process.StandardError.ReadToEnd();
            string stdout = process.StandardOutput.ReadToEnd();

            TimeSpan timeout = config.TestTimeout == TimeSpan.Zero ? TimeSpan.FromMinutes(30) : config.TestTimeout;
            process.WaitForExit((int)timeout.TotalMilliseconds);

            TestRun report;
            using (var stream = File.Open(resultFullpath, FileMode.Open))
            {
                report = (TestRun)TestRunSerializer.Deserialize(stream);
            }

            bool passed = report.ResultSummary.Counters.completed == report.ResultSummary.Counters.passed;
            UnitTestBrowserResult result = new UnitTestBrowserResult
            {
                Attempt = attempt,
                Browser = browser,
                StartTime = report.Results[0].startTime,
                EndTime = report.Results[0].endTime,
                Stdout = stdout,
                Stderr = stderr,
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

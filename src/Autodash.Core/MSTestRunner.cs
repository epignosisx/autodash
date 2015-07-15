﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autodash.Core.MsTest;

namespace Autodash.Core
{
    public class MsTestRunner : IUnitTestRunner
    {
        private static string CommandTemplate =
            "call \"C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\Tools\\VsDevCmd.bat\"" + Environment.NewLine +
            "@set \"PATH=C:\\projects\\autodash\\tools\\;%PATH%\"" + Environment.NewLine +
            "mstest.exe /testcontainer:{0} /test:{1} /resultsfile:\"{2}\"";

        private static readonly XmlSerializer TestRunSerializer = new XmlSerializer(typeof(TestRun));

        public string TestRunnerName { get { return "MSTest Runner"; } }

        public async Task<UnitTestResult> Run(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config)
        {
            if (unitTest == null)
                throw new ArgumentNullException("unitTest");
            if (testCollection == null)
                throw new ArgumentNullException("testCollection");
            if (config == null)
                throw new ArgumentNullException("config");

            UnitTestResult unitTestResult;
            if (config.EnableBrowserExecutionInParallel)
                unitTestResult = await RunInParallel(unitTest, testCollection, config);
            else
                unitTestResult = RunSerially(unitTest, testCollection, config);

            return unitTestResult;
        }

        private static UnitTestResult RunSerially(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config)
        {
            var browserResults = new List<UnitTestBrowserResult>(config.Browsers.Length);
            foreach (var browser in config.Browsers)
            {
                UnitTestBrowserResult browserResult = null;
                do
                {
                    int nextAttempt = browserResult == null ? 1 : (browserResult.Attempt + 1);
                    browserResult = RunTest(unitTest, testCollection, config, browser, nextAttempt);
                    browserResults.Add(browserResult);
                } while (!browserResult.Passed && browserResult.Attempt < config.RetryAttempts);
            }

            var unitTestResult = new UnitTestResult
            {
                TestName = unitTest.TestName,
                BrowserResults = browserResults
            };
            return unitTestResult;
        }

        private static async Task<UnitTestResult> RunInParallel(UnitTestInfo unitTest, UnitTestCollection testCollection,
            TestSuiteConfiguration config)
        {
            var ongoingTests = new List<Task<UnitTestBrowserResult>>(config.Browsers.Length);
            var browserResults = new List<UnitTestBrowserResult>(ongoingTests.Count);
            foreach (var browser in config.Browsers)
            {
                string browserRef = browser;
                ongoingTests.Add(Task.Run(() => RunTest(unitTest, testCollection, config, browserRef, 1)));
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

            var unitTestResult = new UnitTestResult
            {
                TestName = unitTest.TestName,
                BrowserResults = browserResults
            };
            return unitTestResult;
        }

        private static UnitTestBrowserResult RunTest(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config, string browser, int attempt)
        {
            string tempFilename = RemoveInvalidChars(unitTest.ShortTestName + "_" + browser);
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

            Process process = Process.Start(info);
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();

            TimeSpan timeout = config.TestTimeout == TimeSpan.Zero ? TimeSpan.FromMinutes(30) : config.TestTimeout;
            process.WaitForExit((int)timeout.TotalMilliseconds);

            TestRun report;
            using (var stream = File.Open(resultFullpath, FileMode.Open))
            {
                report = (TestRun)TestRunSerializer.Deserialize(stream);
            }

            //clean up
            File.Delete(resultFullpath);
            File.Delete(commandFullpath);

            bool passed = report.ResultSummary.Counters.passed == "1";

            string testOutput = "";
            if(report.Results.Length > 0 && report.Results[0].Output != null)
                testOutput = report.Results[0].Output.StdOut;

            var result = new UnitTestBrowserResult
            {
                Attempt = attempt,
                Browser = browser,
                StartTime = report.Results[0].startTime,
                EndTime = report.Results[0].endTime,
                Stdout = stdout + Environment.NewLine +  "=================" + Environment.NewLine + testOutput,
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

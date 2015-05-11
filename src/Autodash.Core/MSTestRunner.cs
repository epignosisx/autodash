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

        public UnitTestResult Run(UnitTestInfo unitTest, UnitTestCollection testCollection, TestSuiteConfiguration config)
        {
            if (unitTest == null)
                throw new ArgumentNullException("unitTest");
            if (testCollection == null)
                throw new ArgumentNullException("testCollection");
            if (config == null)
                throw new ArgumentNullException("config");
            

            Parallel.ForEach(config.Browsers, browser =>
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

                XmlSerializer serializer = new XmlSerializer(typeof(TestRun));
                using (var stream = File.Open(resultFullpath, FileMode.Open))
                {
                    TestRun t = (TestRun)serializer.Deserialize(stream);
                }
            });

            throw new NotImplementedException();
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

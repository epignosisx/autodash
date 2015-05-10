using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Autodash.Core
{
    public class MsTestRunner : IUnitTestRunner
    {
        private static string CommandTemplate =
            "call \"C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\Common7\\Tools\\VsDevCmd.bat\"" +
            "mstest.exe /testcontainer:{0} /test:{1} /resultsfile:{2}";

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
                string commandFilename = tempFilename + ".bat";
                string resultFilename = tempFilename + ".trx";

                string commandContent = string.Format(CommandTemplate,
                    Path.GetFileName(testCollection.AssemblyPath),
                    unitTest.TestName,
                    resultFilename
                );

                File.AppendAllText(Path.Combine(config.TestAssembliesPath, commandFilename), commandContent);

                ProcessStartInfo info = new ProcessStartInfo();
                info.WorkingDirectory = config.TestAssembliesPath;
                info.FileName = @"C:\Windows\System32\cmd.exe";
                info.Arguments = "/c "
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

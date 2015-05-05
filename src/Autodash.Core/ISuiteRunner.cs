using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Autodash.Core
{
    public interface ISuiteRunner
    {
        Task<SuiteRun> Run(SuiteRun run);
    }

    public class DefaultSuiteRunner : ISuiteRunner
    {
        public Task<SuiteRun> Run(SuiteRun run)
        {
            if (run == null)
                throw new ArgumentNullException("run");

            ValidateRun(run);
            if (run.Result != null)
                return Task.FromResult(run);


        }

        private static void ValidateRun(SuiteRun run)
        {
            var config = run.TestSuiteSnapshot.Configuration;
            string testAssembliesPath = config.TestAssembliesPath;
            if(!Directory.Exists(testAssembliesPath))
            {
                run.Result = new FailedToStartSuiteRunResult("Test assemblies not found at: " + testAssembliesPath);
                return;
            }

            var testAssemblies = Directory.GetFiles(testAssembliesPath)
                .Where(n => string.Equals(Path.GetExtension(n), ".dll", StringComparison.OrdinalIgnoreCase));

            Process p = new Process
            {
                StartInfo = new ProcessStartInfo()
            };

            p.
        }
    }
}
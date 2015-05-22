using System.Globalization;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace Autodash.Core
{
    public class FileSystemTestAssembliesRepository : ITestAssembliesRepository
    {
        private readonly string _repositoryRoot;

        public FileSystemTestAssembliesRepository(string repositoryRoot)
        {
            _repositoryRoot = repositoryRoot;
        }

        public void MoveToTestSuite(TestSuite suite, string zipLocation)
        {
            string suiteLocation = GetSuiteLocation(suite);
            Directory.CreateDirectory(suiteLocation);
            ZipFile.ExtractToDirectory(zipLocation, suiteLocation);
        }

        private string GetSuiteLocation(TestSuite suite)
        {
            string safeName = SafeFileName(suite.Name);
            string fullPath = Path.Combine(_repositoryRoot, suite.Id, safeName);
            return fullPath;
        }

        private static string SafeFileName(string name)
        {
            var illegalChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars());
            foreach(var illegalChar in illegalChars)
            {
                name = name.Replace(illegalChar.ToString(CultureInfo.InvariantCulture), "");
            }
            return name;
        }
    }
}
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

        public string MoveToTestSuite(TestSuite suite, string zipLocation)
        {
            string suiteLocation = GetSuiteLocation(suite);
            Directory.CreateDirectory(suiteLocation);
            ZipFile.ExtractToDirectory(zipLocation, suiteLocation);
            return suiteLocation;
        }

        private string GetSuiteLocation(TestSuite suite)
        {
            string safeName = SafeFileName(suite.Name);
            safeName = Truncate(safeName, 50);
            safeName = suite.Id + "_" + safeName;
            string fullPath = Path.Combine(_repositoryRoot, safeName);
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

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
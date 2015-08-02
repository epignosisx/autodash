using System.Collections.Generic;

namespace Autodash.Core
{
    public interface ITestSuiteFileExplorer
    {
        IEnumerable<string> GetFiles(string testAssembliesPath);
        string GetFileContent(string testAssembliesPath, string fileName);

        void UpdateFileContent(string testAssembliesPath, string fileName, string content);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Autodash.Core
{
    public class XmlOnlyTestSuiteFileExplorer : ITestSuiteFileExplorer
    {
        public IEnumerable<string> GetFiles(string testAssembliesPath)
        {
            if (testAssembliesPath == null) 
                throw new ArgumentNullException("testAssembliesPath");
            return Directory.GetFiles(testAssembliesPath, "*.xml").Select(Path.GetFileName);
        }

        public string GetFileContent(string testAssembliesPath, string fileName)
        {
            Validate(testAssembliesPath, fileName);

            string fullPath = Path.Combine(testAssembliesPath, fileName);
            var xdoc = XDocument.Load(File.OpenRead(fullPath));
            var formatted = xdoc.ToString();
            return formatted;
        }

        public void UpdateFileContent(string testAssembliesPath, string fileName, string content)
        {
            Validate(testAssembliesPath, fileName);

            string fullPath = Path.Combine(testAssembliesPath, fileName);

            //use xdoc to validate it is xml
            var doc = XDocument.Parse(content);
            doc.Save(fullPath);
        }

        private void Validate(string testAssembliesPath, string fileName)
        {
            if (testAssembliesPath == null)
                throw new ArgumentNullException("testAssembliesPath");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            var files = GetFiles(testAssembliesPath);

            if (files.All(n => n != fileName))
                throw new InvalidOperationException("Requested file could not be found.");
        }
    }
}
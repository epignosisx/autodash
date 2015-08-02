using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core
{
    public static class TestRunnerPreProcessorProvider
    {
        private static readonly List<ITestRunnerPreProcessor> PreProcessors = new List<ITestRunnerPreProcessor>();

        public static void Add(ITestRunnerPreProcessor preProcessor)
        {
            if (preProcessor == null) 
                throw new ArgumentNullException("preProcessor");

            PreProcessors.Add(preProcessor);
        }

        public static void Remove(ITestRunnerPreProcessor preProcessor)
        {
            if (preProcessor == null)
                throw new ArgumentNullException("preProcessor");

            PreProcessors.Remove(preProcessor);
        }

        public static IEnumerable<ITestRunnerPreProcessor> GetAll()
        {
            return PreProcessors.Select(n => n);
        }
    }
}

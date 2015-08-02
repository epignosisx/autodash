using System;

namespace Autodash.Core.Tests
{
    public class FakeLoggerProvider : ILoggerProvider
    {
        private static readonly FakeLoggerWrapper Wrapper = new FakeLoggerWrapper();
        public ILoggerWrapper GetLogger(string name)
        {
            return Wrapper;
        }

        public class FakeLoggerWrapper : ILoggerWrapper
        {
            public void Debug(string message)
            {
            }

            public void Info(string message)
            {
            }

            public void Info(string message, params object[] args)
            {
            }

            public void Error(string message)
            {
            }

            public void Error(Exception exception, string message)
            {
            }
        }
    }
}
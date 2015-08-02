using System;
using NLog;

namespace Autodash.Core
{
    public interface ILoggerProvider
    {
        ILoggerWrapper GetLogger(string name);
    }

    public class DefaultLoggerProvider : ILoggerProvider
    {
        public ILoggerWrapper GetLogger(string name)
        {
            return new NLogWrapper(name);
        }
    }

    public interface ILoggerWrapper
    {
        void Debug(string message);
        void Info(string message);
        void Info(string message, params object[] args);
        void Error(string message);
        void Error(Exception exception, string message);
    }

    public class NLogWrapper : ILoggerWrapper
    {
        private readonly Logger _logger;

        public NLogWrapper(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(Exception exception, string message)
        {
            _logger.Error(exception, message);
        }
    }
}

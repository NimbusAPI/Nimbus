using System;
using log4net;
using Nimbus.Infrastructure.Logging;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Logger.Log4net.Logger.Log4Net
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _log;

        public Log4NetLogger(ILog log)
        {
            _log = log;
        }

        public void Debug(string format, params object[] args)
        {
            if (args.Length > 0)
                _log.DebugFormat(StructuredLoggingNormalizer.Normalize(format), args);
            else
                _log.Debug(format);
        }

        public void Info(string format, params object[] args)
        {
            if (args.Length > 0)
                _log.InfoFormat(StructuredLoggingNormalizer.Normalize(format), args);
            else
                _log.Info(format);
        }

        public void Warn(string format, params object[] args)
        {
            if (args.Length > 0)
                _log.WarnFormat(StructuredLoggingNormalizer.Normalize(format), args);
            else
                _log.Warn(format);
        }

        public void Warn(Exception exc, string format, params object[] args)
        {
            _log.Warn(string.Format(format, args), exc);
        }

        public void Error(string format, params object[] args)
        {
            if (args.Length > 0)
                _log.ErrorFormat(StructuredLoggingNormalizer.Normalize(format), args);
            else
                _log.Error(format);
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            var message = (args.Length) > 0 ? String.Format(StructuredLoggingNormalizer.Normalize(format), args) : format;
            _log.Error(message, exc);
        }
    }
}
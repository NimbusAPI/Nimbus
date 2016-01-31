using System;
using log4net;
using Nimbus.Infrastructure.Logging;

namespace Nimbus.Logger.Log4net
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
            _log.DebugFormat(format.NormalizeToStringFormat(), args);
        }

        public void Info(string format, params object[] args)
        {
            _log.InfoFormat(format.NormalizeToStringFormat(), args);
        }

        public void Warn(string format, params object[] args)
        {
            _log.WarnFormat(format.NormalizeToStringFormat(), args);
        }

        public void Warn(Exception exc, string format, params object[] args)
        {
            _log.Warn(string.Format(format.NormalizeToStringFormat(), args), exc);
        }

        public void Error(string format, params object[] args)
        {
            _log.ErrorFormat(format.NormalizeToStringFormat(), args);
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            var message = string.Format(format.NormalizeToStringFormat(), args);
            _log.Error(message, exc);
        }
    }
}
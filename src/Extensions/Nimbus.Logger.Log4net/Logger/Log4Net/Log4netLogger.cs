using System;
using log4net;

namespace Nimbus.Logger.Log4net
{
    public class Log4NetLogger : Nimbus.ILogger
    {
        private readonly ILog _log;

        public Log4NetLogger(ILog log)
        {
            _log = log;
        }

        public void Debug(string format, params object[] args)
        {
            _log.DebugFormat(format, args);
        }

        public void Info(string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _log.WarnFormat(format, args);
        }

        public void Error(string format, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            var message = String.Format(format, args);
            _log.Error(message, exc);
        }

    }
}

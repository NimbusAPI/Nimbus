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
            if (args.Length > 0)
                _log.DebugFormat(format, args);
            else
                _log.Debug(format);
        }

        public void Info(string format, params object[] args)
        {
            if (args.Length > 0)
                _log.InfoFormat(format, args);
            else
                _log.Info(format);
        }

        public void Warn(string format, params object[] args)
        {
            if (args.Length > 0)
                _log.WarnFormat(format, args);
            else
                _log.Warn(format);
        }

        public void Error(string format, params object[] args)
        {
            if (args.Length > 0)
                _log.ErrorFormat(format, args);
            else
                _log.Error(format);
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            var message = (args.Length) > 0 ? String.Format(format, args) : format;
            _log.Error(message, exc);
        }
    }
}

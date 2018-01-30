using System;
using ISerilogLogger = Serilog.ILogger;

namespace Nimbus.Logger.Serilog
{
    public class SerilogLogger : ILogger
    {
        private readonly ISerilogLogger _logger;

        public SerilogLogger(ISerilogLogger logger)
        {
            _logger = logger;
        }

        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        public void Info(string format, params object[] args)
        {
            _logger.Information(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _logger.Warning(format, args);
        }

        public void Warn(Exception exc, string format, params object[] args)
        {
            _logger.Warning(exc, format, args);
        }

        public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            _logger.Error(exc, format, args);
        }
    }
}
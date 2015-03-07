using System;
using NLogger = NLog.Logger;

namespace Nimbus.Logger.NLog
{
	public class NLogLogger : ILogger
	{
		private readonly NLogger _logger;

		public NLogLogger(NLogger logger)
		{
			_logger = logger;
		}

		public void Debug(string format, params object[] args)
		{
			_logger.Debug(format, args);
		}

		public void Info(string format, params object[] args)
		{
			_logger.Info(format, args);
		}

		public void Warn(string format, params object[] args)
		{
			_logger.Warn(format, args);
		}

		public void Error(string format, params object[] args)
		{
			_logger.Error(format, args);
		}

		public void Error(Exception exc, string format, params object[] args)
		{
			_logger.Log(new global::NLog.LogEventInfo() { Message = format, Parameters = args, Exception = exc, Level = global::NLog.LogLevel.Error });
		}
	}
}

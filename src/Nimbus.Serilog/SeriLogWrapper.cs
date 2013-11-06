using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace Nimbus.Serilog
{
    internal class SeriLogWrapper : Nimbus.Logger.ILogger, ILogger
    {
        private readonly ILogger _logger;

        internal SeriLogWrapper(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger ForContext(IEnumerable<ILogEventEnricher> enrichers)
        {
            return _logger.ForContext(enrichers);
        }

        public ILogger ForContext(string propertyName, object value, bool destructureObjects = false)
        {
            return _logger.ForContext(propertyName, value, destructureObjects);
        }

        public ILogger ForContext<TSource>()
        {
            return _logger.ForContext<TSource>();
        }

        public ILogger ForContext(Type source)
        {
            return _logger.ForContext(source);
        }

        public void Write(LogEvent logEvent)
        {
            _logger.Write(logEvent);
        }

        public void Write(LogEventLevel level, string messageTemplate, params object[] propertyValues)
        {
            _logger.Write(level, messageTemplate, propertyValues);
        }

        public void Write(LogEventLevel level, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Write(level, exception, messageTemplate, propertyValues);
        }

        public bool IsEnabled(LogEventLevel level)
        {
            return _logger.IsEnabled(level);
        }

        public void Verbose(string messageTemplate, params object[] propertyValues)
        {
            _logger.Verbose(messageTemplate, propertyValues);
        }

        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Verbose(exception, messageTemplate, propertyValues);
        }

        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            _logger.Debug(messageTemplate, propertyValues);
        }

        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Debug(exception, messageTemplate, propertyValues);
        }

        public void Information(string messageTemplate, params object[] propertyValues)
        {
            _logger.Verbose(messageTemplate, propertyValues);
        }

        public void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Information(exception, messageTemplate, propertyValues);
        }

        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            _logger.Warning(messageTemplate, propertyValues);
        }

        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Warning(exception, messageTemplate, propertyValues);
        }

        public void Error(string messageTemplate, params object[] propertyValues)
        {
            _logger.Error(messageTemplate, propertyValues);
        }

        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Error(exception, messageTemplate, propertyValues);
        }

        public void Fatal(string messageTemplate, params object[] propertyValues)
        {
            _logger.Fatal(messageTemplate, propertyValues);
        }

        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Fatal(exception, messageTemplate, propertyValues);
        }
    }
}

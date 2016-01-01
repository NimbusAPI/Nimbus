using System;

namespace Nimbus.Infrastructure.Logging
{
    public class ConsoleLogger : ILogger
    {
        private static readonly object _mutex = new object();
        public static Func<DateTimeOffset> TimestampFunc = () => DateTimeOffset.UtcNow;

        public void Debug(string format, params object[] args)
        {
            lock (_mutex)
            {
                OutputMessage(format, args);
            }
        }

        public void Info(string format, params object[] args)
        {
            lock (_mutex)
            {
                OutputMessage(format, args);
            }
        }

        public void Warn(string format, params object[] args)
        {
            lock (_mutex)
            {
                OutputMessage(format, args);
            }
        }

        public void Warn(Exception exc, string format, params object[] args)
        {
            lock(_mutex)
            {
                OutputMessage(format, args);
                Console.WriteLine(exc.ToString());
            }
        }

        public void Error(string format, params object[] args)
        {
            lock (_mutex)
            {
                OutputMessage(format, args);
            }
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            lock (_mutex)
            {
                OutputMessage(format, args);
                Console.WriteLine(exc.ToString());
            }
        }

        private static void OutputMessage(string format, object[] args)
        {
            var prefix = TimestampFunc().ToLocalTime().ToString();
            var normalizedFormat = format.NormalizeToStringFormat();
            var message = string.Format(normalizedFormat, args);
            Console.WriteLine("{0}: {1}", prefix, message);
        }
    }
}
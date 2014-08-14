using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.IntegrationTests.Tests.ThreadStarvationTests
{
    public class RollingLogger : ILogger
    {
        private readonly List<string> _messages = new List<string>();
        private int _lineCount;

        public event EventHandler<string> Storing;

        public void Debug(string format, params object[] args)
        {
            Store(string.Format(format, args));
        }

        public void Info(string format, params object[] args)
        {
            Store(string.Format(format, args));
        }

        public void Warn(string format, params object[] args)
        {
            Store(string.Format(format, args));
        }

        public void Error(string format, params object[] args)
        {
            Store(string.Format(format, args));
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            Store(string.Format(format, args));
            Store(string.Format(exc.ToString()));
        }

        private void Store(string message)
        {
            var prefix = DateTimeOffset.UtcNow.ToLocalTime().ToString();
            Console.WriteLine("{0}: {1}", prefix, message);

            lock (_messages)
            {
                //if (_messages.Count > 1000) _messages.RemoveAt(0);
                _messages.Add(message);
                _lineCount++;
            }

            var handler = Storing;
            if (handler != null) handler(this, message);
        }

        public void Dump()
        {
            foreach (var message in _messages.AsEnumerable().Reverse())
            {
                Console.WriteLine(message);
            }

            Console.WriteLine("Total lines of logs: {0}", _lineCount);
        }
    }
}
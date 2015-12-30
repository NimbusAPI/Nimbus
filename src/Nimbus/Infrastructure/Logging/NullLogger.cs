using System;

namespace Nimbus.Infrastructure.Logging
{
    public class NullLogger : ILogger
    {
        public void Debug(string format, params object[] args)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void Warn(Exception exc, string format, params object[] args)
        {
        }

        public void Error(string format, params object[] args)
        {
        }

        public void Error(Exception exc, string format, params object[] args)
        {
        }
    }
}
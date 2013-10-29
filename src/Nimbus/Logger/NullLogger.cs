using System;

namespace Nimbus.Logger
{
    internal class NullLogger : ILogger
    {
        public void Verbose(string format, params object[] args)
        {
            
        }

        public void Verbose(Exception exception, string format, params object[] args)
        {
            
        }

        public void Debug(string format, params object[] args)
        {
            
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            
        }

        public void Information(string format, params object[] args)
        {
            
        }

        public void Information(Exception exception, string format, params object[] args)
        {
            
        }

        public void Warning(string format, params object[] args)
        {
            
        }

        public void Warning(Exception exception, string format, params object[] args)
        {
            
        }

        public void Error(string format, params object[] args)
        {
            
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            
        }

        public void Fatal(string format, params object[] args)
        {
            
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
            
        }
    }
}
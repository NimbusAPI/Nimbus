using System;
using Serilog;

namespace Nimbus.Logger.Serilog
{
    public class SerilogStaticLogger : ILogger
    {
        public void Debug(string format, params object[] args)
        {
            Log.Debug(format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log.Information(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Log.Warning(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log.Error(format, args);
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            Log.Error(exc, format, args);
        }
    }
}
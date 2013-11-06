using System;

namespace Nimbus.Logger
{
    public interface ILogger
    {
        void Verbose(string format, params object[] args);
        void Verbose(Exception exception, string format, params object[] args);
        
        void Debug(string format, params object[] args);
        void Debug(Exception exception, string format, params object[] args);
        
        void Information(string format, params object[] args);
        void Information(Exception exception, string format, params object[] args);
        
        void Warning(string format, params object[] args);
        void Warning(Exception exception, string format, params object[] args);
        
        void Error(string format, params object[] args);
        void Error(Exception exception, string format, params object[] args);
        
        void Fatal(string format, params object[] args);
        void Fatal(Exception exception, string format, params object[] args);
    }
}
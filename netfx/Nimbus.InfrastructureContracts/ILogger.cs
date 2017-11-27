using System;

namespace Nimbus
{
    public interface ILogger
    {
        void Debug(string format, params object[] args);
        void Info(string format, params object[] args);
        void Warn(string format, params object[] args);
        void Warn(Exception exc, string format, params object[] args);
        void Error(string format, params object[] args);
        void Error(Exception exc, string format, params object[] args);
    }
}
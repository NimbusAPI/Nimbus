using System;
using System.Diagnostics;

namespace Nimbus.Infrastructure.Logging
{
    public class TraceLogger : ILogger
    {
        public void Debug(string format, params object[] args)
        {
            Trace.WriteLine(String.Format(format, args));
        }

        public void Info(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }

        public void Warn(Exception exc, string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
            if (exc == null) return;

            string exceptionMessage;
            if (exc.InnerException != null)
            {
                exceptionMessage = String.Format("{0}{1}{2}{1}{3}{1}{4}",
                                                 exc.Message,
                                                 Environment.NewLine,
                                                 exc.StackTrace,
                                                 exc.InnerException.Message,
                                                 exc.InnerException.StackTrace);
            }
            else
            {
                exceptionMessage = string.Format("{0}{1}{2}",
                                                 exc.Message,
                                                 Environment.NewLine,
                                                 exc.StackTrace);
            }
            Trace.TraceError(exceptionMessage);
        }

        public void Error(string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            Trace.TraceError(format, args);
            if (exc == null) return;

            string exceptionMessage;
            if (exc.InnerException != null)
            {
                exceptionMessage = String.Format("{0}{1}{2}{1}{3}{1}{4}",
                                                 exc.Message,
                                                 Environment.NewLine,
                                                 exc.StackTrace,
                                                 exc.InnerException.Message,
                                                 exc.InnerException.StackTrace);
            }
            else
            {
                exceptionMessage = string.Format("{0}{1}{2}",
                                                 exc.Message,
                                                 Environment.NewLine,
                                                 exc.StackTrace);
            }
            Trace.TraceError(exceptionMessage);
        }
    }
}
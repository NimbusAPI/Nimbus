using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Nimbus.Infrastructure
{
    public static class ExceptionExtensions
    {
        public static Dictionary<string, object> ExceptionDetailsAsProperties(this Exception exception, DateTimeOffset timestamp)
        {
            if (exception is TargetInvocationException || exception is AggregateException) return ExceptionDetailsAsProperties(exception.InnerException, timestamp);

            return new Dictionary<string, object>
                   {
                       {MessagePropertyKeys.ExceptionType, exception.GetType().FullName},
                       {MessagePropertyKeys.ExceptionMessage, exception.Message},
                       {MessagePropertyKeys.ExceptionStackTrace, exception.StackTrace},
                       {MessagePropertyKeys.ExceptionTimestamp, timestamp.ToString()},
                       {MessagePropertyKeys.ExceptionMachineName, Environment.MachineName},
                       {MessagePropertyKeys.ExceptionIdentityName, GetCurrentIdentityName()},
                   };
        }

        private static string GetCurrentIdentityName()
        {
            return (Thread.CurrentPrincipal != null &&
                    Thread.CurrentPrincipal.Identity != null &&
                    Thread.CurrentPrincipal.Identity.IsAuthenticated)
                ? Thread.CurrentPrincipal.Identity.Name
                : "";
        }
    }
}
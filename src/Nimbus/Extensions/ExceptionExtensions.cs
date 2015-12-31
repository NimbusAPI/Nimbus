using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Nimbus.Infrastructure;

namespace Nimbus.Extensions
{
    internal static class ExceptionExtensions
    {
        internal static Dictionary<string, object> ExceptionDetailsAsProperties(this Exception exception, DateTimeOffset timestamp)
        {
            if (exception is TargetInvocationException || exception is AggregateException) return ExceptionDetailsAsProperties(exception.InnerException, timestamp);

            return new Dictionary<string, object>
                   {
                       {MessagePropertyKeys.ExceptionType, exception.GetType().FullName},
                       {MessagePropertyKeys.ExceptionMessage, exception.Message},
                       {MessagePropertyKeys.ExceptionStackTrace, exception.StackTrace},
                       {MessagePropertyKeys.ExceptionTimestamp, timestamp.ToString()},
                       {MessagePropertyKeys.ExceptionMachineName, Environment.MachineName},
                       {MessagePropertyKeys.ExceptionIdentityName, GetCurrentIdentityName()}
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

        internal static T WithData<T>(this T exception, string key, object value) where T : Exception
        {
            exception.Data[key] = value;
            return exception;
        }
    }
}
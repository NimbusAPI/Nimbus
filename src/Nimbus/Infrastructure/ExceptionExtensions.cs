using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nimbus.Infrastructure
{
    public static class ExceptionExtensions
    {
        public static Dictionary<string, object> ExceptionDetailsAsProperties(this Exception exception)
        {
            if (exception is TargetInvocationException || exception is AggregateException) return ExceptionDetailsAsProperties(exception.InnerException);

            return new Dictionary<string, object>
                   {
                       {MessagePropertyKeys.ExceptionTypeKey, exception.GetType().FullName},
                       {MessagePropertyKeys.ExceptionMessageKey, exception.Message},
                       {MessagePropertyKeys.ExceptionStackTraceKey, exception.StackTrace},
                   };
        }
    }
}
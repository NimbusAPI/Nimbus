using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;

namespace Nimbus.Tests.Common
{
    public static class MethodCallCounter
    {
        private static readonly ThreadSafeDictionary<string, ConcurrentBag<object[]>> _allReceivedCalls = new ThreadSafeDictionary<string, ConcurrentBag<object[]>>();
        private static bool _stopped;

        public static void RecordCall<T>(Expression<Action<T>> expr)
        {
            if (_stopped) throw new InvalidOperationException("{0} was not expecting any more calls!".FormatWith((typeof (MethodCallCounter).Name)));

            var methodCallExpression = (MethodCallExpression) expr.Body;
            var method = methodCallExpression.Method;

            // http://stackoverflow.com/questions/2616638/access-the-value-of-a-member-expression
            var args = new List<object>();
            foreach (var argExpr in methodCallExpression.Arguments)
            {
                var messageExpression = argExpr;
                var objectMember = Expression.Convert(messageExpression, typeof (object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                var arg = getter();
                args.Add(arg);
            }

            var key = GetMethodKey(typeof (T), method);
            var methodCallBag = _allReceivedCalls.GetOrAdd(key, k => new ConcurrentBag<object[]>());
            methodCallBag.Add(args.ToArray());

            Console.WriteLine("Observed call to {0}".FormatWith(key));
        }

        public static IEnumerable<KeyValuePair<string, object[]>> AllReceivedCalls
        {
            get
            {
                var callsGroupedByMethodName = _allReceivedCalls
                    .ToDictionary();

                foreach (var methodName in callsGroupedByMethodName.Keys)
                {
                    foreach (var callWithArgs in callsGroupedByMethodName[methodName])
                    {
                        yield return new KeyValuePair<string, object[]>(methodName, callWithArgs);
                    }
                }
            }
        }

        public static IEnumerable<object[]> AllReceivedCallArgs
        {
            get
            {
                var messageBags = _allReceivedCalls.ToDictionary().Values;

                var messages = messageBags
                    .SelectMany(c => c)
                    .ToArray();

                return messages;
            }
        }

        public static IEnumerable<object> AllReceivedMessages
        {
            get
            {
                var messageBags = _allReceivedCalls.ToDictionary().Values;

                var messages = messageBags
                    .SelectMany(c => c)
                    .SelectMany(args => args)
                    .ToArray();
                return messages;
            }
        }

        public static IEnumerable<object[]> ReceivedCallsWithAnyArg<T>(Expression<Action<T>> expr)
        {
            var methodCallExpression = (MethodCallExpression) expr.Body;
            var method = methodCallExpression.Method;
            var key = GetMethodKey(typeof (T), method);
            var messageBag = _allReceivedCalls.GetOrAdd(key, k => new ConcurrentBag<object[]>());
            return messageBag;
        }

        public static int TotalReceivedCalls
        {
            get { return AllReceivedCalls.Count(); }
        }

        public static void Clear()
        {
            _allReceivedCalls.Clear();
            _stopped = false;
        }

        public static void Stop()
        {
            _stopped = true;
        }

        private static string GetMethodKey(Type type, MethodInfo method)
        {
            var parameters = method
                .GetParameters()
                .Select(p => "{0} {1}".FormatWith(p.ParameterType, p.Name))
                .ToArray();

            var parameterString = string.Join(", ", parameters);

            var key = "{0}.{1}({2})".FormatWith(type.FullName, method.Name, parameterString);
            return key;
        }

        public static void Dump()
        {
            var allReceivedCalls = _allReceivedCalls
                .ToDictionary();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Total calls observed: {0}", allReceivedCalls.Values.SelectMany(v => v).Count());

            foreach (var kvp in allReceivedCalls)
            {
                foreach (var methodCall in kvp.Value)
                {
                    Console.WriteLine("\t{0}({1})", kvp.Key, string.Join(", ", methodCall));
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
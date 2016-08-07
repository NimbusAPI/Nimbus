using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;
using Serilog;

namespace Nimbus.Tests.Common.TestUtilities
{
    public class MethodCallCounter
    {
        private static readonly Dictionary<Guid, MethodCallCounter> _instances = new Dictionary<Guid, MethodCallCounter>();

        private readonly ThreadSafeDictionary<string, ConcurrentBag<object[]>> _allReceivedCalls = new ThreadSafeDictionary<string, ConcurrentBag<object[]>>();
        private bool _stopped;
        private int _callCount;

        private MethodCallCounter()
        {
        }

        public static MethodCallCounter CreateInstance(Guid instanceId)
        {
            var instance = new MethodCallCounter();
            _instances.Add(instanceId, instance);
            return instance;
        }

        public static MethodCallCounter ForInstance(Guid instanceId)
        {
            return _instances[instanceId];
        }

        public static void DestroyInstance(Guid instanceId)
        {
            _instances.Remove(instanceId);
        }

        public void RecordCall<T>(Expression<Action<T>> expr)
        {
            if (_stopped) throw new InvalidOperationException("{0} was not expecting any more calls!".FormatWith((typeof(MethodCallCounter).Name)));

            var methodCallExpression = (MethodCallExpression) expr.Body;
            var method = methodCallExpression.Method;

            // http://stackoverflow.com/questions/2616638/access-the-value-of-a-member-expression
            var args = new List<object>();
            foreach (var argExpr in methodCallExpression.Arguments)
            {
                var messageExpression = argExpr;
                var objectMember = Expression.Convert(messageExpression, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                var arg = getter();
                args.Add(arg);
            }

            var key = GetMethodKey(typeof(T), method);
            var methodCallBag = _allReceivedCalls.GetOrAdd(key, k => new ConcurrentBag<object[]>());
            methodCallBag.Add(args.ToArray());

            var callCount = Interlocked.Increment(ref _callCount);
            Log.Information("Observed call {CallCount} to {HandlerMethod}", callCount, key);
        }

        public IEnumerable<KeyValuePair<string, object[]>> AllReceivedCalls
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

        public IEnumerable<object[]> AllReceivedCallArgs
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

        public IEnumerable<object> AllReceivedMessages
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

        public IEnumerable<object[]> ReceivedCallsWithAnyArg<T>(Expression<Action<T>> expr)
        {
            var methodCallExpression = (MethodCallExpression) expr.Body;
            var method = methodCallExpression.Method;
            var key = GetMethodKey(typeof(T), method);
            var messageBag = _allReceivedCalls.GetOrAdd(key, k => new ConcurrentBag<object[]>());
            return messageBag;
        }

        public int TotalReceivedCalls
        {
            get { return AllReceivedCalls.Count(); }
        }

        public void Clear()
        {
            _callCount = 0;
            _allReceivedCalls.Clear();
            _stopped = false;
        }

        public void Stop()
        {
            _stopped = true;
        }

        private string GetMethodKey(Type type, MethodInfo method)
        {
            var parameters = method
                .GetParameters()
                .Select(p => "{0} {1}".FormatWith(p.ParameterType, p.Name))
                .ToArray();

            var parameterString = string.Join(", ", parameters);

            var key = "{0}.{1}({2})".FormatWith(type.FullName, method.Name, parameterString);
            return key;
        }

        public void Dump()
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

            Console.WriteLine("Total calls observed: {0}", allReceivedCalls.Values.SelectMany(v => v).Count());
        }
    }
}
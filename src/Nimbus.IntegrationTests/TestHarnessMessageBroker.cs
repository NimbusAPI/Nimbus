using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.IntegrationTests
{
    public class TestHarnessMessageBroker : DefaultMessageBroker
    {
        private readonly ConcurrentDictionary<MethodInfo, ConcurrentBag<object>> _allReceivedCalls = new ConcurrentDictionary<MethodInfo, ConcurrentBag<object>>();

        public TestHarnessMessageBroker(ITypeProvider typeProvider) : base(typeProvider)
        {
            Console.WriteLine("{0} created".FormatWith(GetType().Name));
        }

        public override void Dispatch<TBusCommand>(TBusCommand busCommand)
        {
            RecordCall(mb => mb.Dispatch(busCommand));

            base.Dispatch(busCommand);
        }

        public override TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request)
        {
            RecordCall(mb => mb.Handle<TBusRequest, TBusResponse>(request));
            return base.Handle<TBusRequest, TBusResponse>(request);
        }

        public override IEnumerable<TBusResponse> HandleMulticast<TBusRequest, TBusResponse>(TBusRequest request, TimeSpan timeout)
        {
            RecordCall(mb => mb.HandleMulticast<TBusRequest, TBusResponse>(request, timeout));
            return base.HandleMulticast<TBusRequest, TBusResponse>(request, timeout);
        }

        public override void PublishMulticast<TBusEvent>(TBusEvent busEvent)
        {
            RecordCall(mb => mb.PublishMulticast(busEvent));
            base.PublishMulticast(busEvent);
        }

        public override void PublishCompeting<TBusEvent>(TBusEvent busEvent)
        {
            RecordCall(mb => mb.PublishCompeting(busEvent));

            base.PublishCompeting(busEvent);
        }

        public IEnumerable<KeyValuePair<MethodInfo, ConcurrentBag<object>>> AllReceivedCalls
        {
            get { return _allReceivedCalls; }
        }

        public IEnumerable<object> AllReceivedMessages
        {
            get
            {
                var messageBags = _allReceivedCalls.Values;
                var messages = messageBags.SelectMany(bag => bag).ToArray();
                return messages;
            }
        }

        public IEnumerable<object> ReceivedCallsWithAnyArg(Expression<Action<TestHarnessMessageBroker>> expr)
        {
            var methodCallExpression = (MethodCallExpression) expr.Body;
            var method = methodCallExpression.Method;
            var messageBag = _allReceivedCalls.GetOrAdd(method, new ConcurrentBag<object>());
            return messageBag;
        }

        private void RecordCall(Expression<Action<TestHarnessMessageBroker>> expr)
        {
            var methodCallExpression = (MethodCallExpression) expr.Body;
            var method = methodCallExpression.Method;

            // http://stackoverflow.com/questions/2616638/access-the-value-of-a-member-expression
            var messageExpression = methodCallExpression.Arguments.First();
            var objectMember = Expression.Convert(messageExpression, typeof (object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            var message = getter();

            var messageBag = _allReceivedCalls.GetOrAdd(method, new ConcurrentBag<object>());
            messageBag.Add(message);

            Console.WriteLine("Observed call to {0} with argument of type {1}".FormatWith(method.Name, message.GetType()));
        }
    }
}
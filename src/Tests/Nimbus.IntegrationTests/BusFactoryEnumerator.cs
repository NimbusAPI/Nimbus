using System;
using System.Collections.Generic;
using Nimbus.Infrastructure;
using Nimbus.IntegrationTests.InfrastructureContracts;

namespace Nimbus.IntegrationTests
{
    public class BusFactoryEnumerator
    {
        private readonly Type _testFixtureType;

        public BusFactoryEnumerator(Type testFixtureType)
        {
            _testFixtureType = testFixtureType;
        }

        public IEnumerable<ITestHarnessBusFactory> GetBusFactories()
        {
            // Filter types we care about to only our own test's namespace. It's a performance optimisation because creating and
            // deleting queues and topics is slow.
            var typeProvider = new TestHarnessTypeProvider(new[] {_testFixtureType.Assembly}, new[] {_testFixtureType.Namespace});

            yield return new DefaultBusFactory(typeProvider);
        }

        public class DefaultBusFactory : ITestHarnessBusFactory
        {
            private readonly TestHarnessTypeProvider _typeProvider;

            public DefaultBusFactory(TestHarnessTypeProvider typeProvider)
            {
                _typeProvider = typeProvider;
            }

            public string MessageBrokerName
            {
                get { return "DefaultMessageBroker"; }
            }

            public IBus Create()
            {
                var messageBroker = new DefaultMessageHandlerFactory(_typeProvider);

                var bus = TestHarnessBusFactory.CreateAndStart(_typeProvider, messageBroker);
                return bus;
            }
        }
    }
}
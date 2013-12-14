using System;
using System.Collections.Generic;
using Nimbus.Infrastructure;

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
            yield return new DefaultBusFactory(_testFixtureType);
        }

        public class DefaultBusFactory : ITestHarnessBusFactory
        {
            private readonly Type _testFixtureType;

            public DefaultBusFactory(Type testFixtureType)
            {
                _testFixtureType = testFixtureType;
            }

            public string BusFactoryName
            {
                get { return "DefaultBusFactory"; }
            }

            public IBus Create()
            {
                // Filter types we care about to only our own test's namespace. It's a performance optimisation because creating and
                // deleting queues and topics is slow.
                var typeProvider = new TestHarnessTypeProvider(new[] {_testFixtureType.Assembly}, new[] {_testFixtureType.Namespace});

                var messageBroker = new DefaultMessageBroker(typeProvider);

                var bus = TestHarnessBusFactory.CreateAndStart(typeProvider, messageBroker);
                return bus;
            }
        }
    }
}
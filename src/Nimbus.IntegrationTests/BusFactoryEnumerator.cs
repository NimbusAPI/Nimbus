using System;
using System.Collections.Generic;
using Nimbus.Infrastructure;
using Nimbus.IntegrationTests.Autofac;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Windsor;

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
            yield return new AutofacBusFactory(typeProvider, CommonResources.ConnectionString);
            yield return new WindsorBusFactory(typeProvider, CommonResources.ConnectionString);
        }

        public class DefaultBusFactory : ITestHarnessBusFactory
        {
            private readonly TestHarnessTypeProvider _typeProvider;

            public DefaultBusFactory(TestHarnessTypeProvider typeProvider)
            {
                _typeProvider = typeProvider;
            }

            public string BusFactoryName
            {
                get { return "DefaultBusFactory"; }
            }

            public IBus Create()
            {
                var messageBroker = new DefaultMessageBroker(_typeProvider);

                var bus = TestHarnessBusFactory.CreateAndStart(_typeProvider, messageBroker);
                return bus;
            }
        }
    }
}
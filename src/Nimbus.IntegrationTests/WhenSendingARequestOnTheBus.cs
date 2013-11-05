using System;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.MessageContracts;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public class WhenSendingARequestOnTheBus : SpecificationFor<Bus>
    {
        public class FakeBroker : IRequestBroker
        {
            public bool DidGetCalled;

            public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
            {
                DidGetCalled = true;

                return Activator.CreateInstance<TBusResponse>();
            }
        }

        private SomeResponse _response;

        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = new FakeBroker();
            _eventBroker = Substitute.For<IEventBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            var bus = new BusBuilder().Configure()
                                      .WithInstanceName(Environment.MachineName + ".MyTestSuite")
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(_commandBroker)
                                      .WithRequestBroker(_requestBroker)
                                      .WithEventBroker(_eventBroker)
                                      .WithDefaultTimeout(_defaultTimeout)
                                      .Build();
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var request = new SomeRequest();
            var task = Subject.Request(request);
            _response = task.WaitForResult(_defaultTimeout);

            Subject.Stop();
        }

        [Test]
        public void WeShouldGetSomethingNiceBack()
        {
            _response.ShouldNotBe(null);
        }
    }
}
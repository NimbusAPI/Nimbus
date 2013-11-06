using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.Exceptions;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.MessageContracts;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests.ExceptionPropagation
{
    public class WhenSendingARequestThatWillThrow : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);
        private SomeResponse _response;

        public class BrokenRequestBroker : IRequestBroker
        {
            public const string ExceptionMessage = "This is supposed to go bang.";

            [DebuggerStepThrough]
            public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
            {
                throw new Exception(ExceptionMessage);
            }
        }

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = new BrokenRequestBroker();
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
        }

        [Test]
        public void AnExceptionShouldBeReThrownOnTheClient()
        {
            var timeout = TimeSpan.FromSeconds(60);
            var request = new SomeRequest();
            var task = Subject.Request(request, timeout);

            try
            {
                _response = task.WaitForResult(timeout);
            }
            catch (AggregateException aexc)
            {
                var exc = aexc.Flatten().InnerExceptions.Single();
                exc.ShouldBeTypeOf<RequestFailedException>();
                exc.Message.ShouldBe(BrokenRequestBroker.ExceptionMessage);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Subject.Stop();
        }
    }
}
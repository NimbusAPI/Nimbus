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
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.ExceptionPropagationTests
{
    public class WhenSendingARequestThatWillThrow : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IMulticastEventBroker _multicastEventBroker;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);
        private SomeResponse _response;
        private ICompetingEventBroker _competingEventBroker;

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
            _multicastEventBroker = Substitute.For<IMulticastEventBroker>();
            _competingEventBroker = Substitute.For<ICompetingEventBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(_commandBroker)
                                      .WithRequestBroker(_requestBroker)
                                      .WithMulticastEventBroker(_multicastEventBroker)
                                      .WithCompetingEventBroker(_competingEventBroker)
                                      .WithDefaultTimeout(_defaultTimeout)
                                      .WithDebugOptions(dc =>
                                                        dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                            "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
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
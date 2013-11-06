﻿using System;
using System.Linq;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.MessageContracts;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public class WhenSendingACommandOnTheBus : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _eventBroker = Substitute.For<IEventBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            var bus = new BusBuilder().Configure()
                                      .WithInstanceName(Environment.MachineName + ".MyTestSuite")
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(_commandBroker)
                                      .WithRequestBroker(_requestBroker)
                                      .WithEventBroker(_eventBroker)
                                      .Build();
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var someCommand = new SomeCommand();
            Subject.Send(someCommand).Wait();
            TimeSpan.FromSeconds(1).SleepUntil(() => _commandBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void SomethingShouldHappen()
        {
            _commandBroker.Received().Dispatch(Arg.Any<SomeCommand>());
        }
    }
}
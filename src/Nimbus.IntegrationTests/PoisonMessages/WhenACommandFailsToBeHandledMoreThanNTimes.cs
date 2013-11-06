using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Extensions;
using Shouldly;

namespace Nimbus.IntegrationTests.PoisonMessages
{
    [TestFixture]
    public class WhenACommandFailsToBeHandledMoreThanNTimes : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;
        private TestCommand _testCommand;
        private string _someContent;
        private List<TestCommand> _deadLetterMessages = new List<TestCommand>();

        private const int _maxDeliveryAttempts = 7;

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _commandBroker.When(cb => cb.Dispatch(Arg.Any<TestCommand>()))
                          .Do(callInfo => new TestCommandHandler().Handle(callInfo.Arg<TestCommand>()));

            _requestBroker = Substitute.For<IRequestBroker>();
            _eventBroker = Substitute.For<IEventBroker>();

            var typeProvider = new TestTypesProvider(new[] { typeof(TestCommandHandler) }, new[] { typeof(TestCommand) }, new Type[0], new Type[0], new Type[0], new Type[0], new Type[0], new Type[0]);

            var bus = new BusBuilder().Configure()
                                      .WithInstanceName(Environment.MachineName + ".MyTestSuite")
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(_commandBroker)
                                      .WithRequestBroker(_requestBroker)
                                      .WithEventBroker(_eventBroker)
                                      .WithMaxDeliveryAttempts(_maxDeliveryAttempts)
                                      .Build();
            bus.Start();
            return bus;
        }

        public override void When()
        {
            _someContent = Guid.NewGuid().ToString();
            _testCommand = new TestCommand(_someContent);

            FetchAllDeadLetterMessages().WaitForResult(); // clear the dead letter queue
            Subject.Send(_testCommand).Wait();
            TimeSpan.FromSeconds(10).SleepUntil(() => _commandBroker.ReceivedCalls().Count() >= _maxDeliveryAttempts);
            _deadLetterMessages = FetchAllDeadLetterMessages().WaitForResult();
        }

        [Test]
        public void TheCommandBrokerShouldHaveBeenCalledExactlyNTimes()
        {
            _commandBroker.Received(_maxDeliveryAttempts).Dispatch(Arg.Any<TestCommand>());
        }

        [Test]
        public void ThereShouldBeExactlyOneMessageOnTheDeadLetterQueue()
        {
            _deadLetterMessages.Count.ShouldBe(1);
            _deadLetterMessages.Single().SomeContent.ShouldBe(_someContent);
        }

        private async Task<List<TestCommand>> FetchAllDeadLetterMessages()
        {
            var deadLetterMessages = new List<TestCommand>();
            while (true)
            {
                var message = await Subject.DeadLetterQueues.CommandQueue.Pop<TestCommand>();
                if (message == null) break;
                deadLetterMessages.Add(message);
            }
            return deadLetterMessages;
        }
    }
}
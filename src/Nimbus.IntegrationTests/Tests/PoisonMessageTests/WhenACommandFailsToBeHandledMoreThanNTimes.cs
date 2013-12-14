using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests
{
    [TestFixture]
    public class WhenACommandFailsToBeHandledMoreThanNTimes : TestForAllBuses
    {
        private TestCommand _testCommand;
        private string _someContent;
        private List<TestCommand> _deadLetterMessages = new List<TestCommand>();

        private const int _maxDeliveryAttempts = 7;

        public override async Task When(ITestHarnessBusFactory busFactory)
        {
            var bus = busFactory.Create();

            _someContent = Guid.NewGuid().ToString();
            _testCommand = new TestCommand(_someContent);

            await bus.Send(_testCommand);
            TimeSpan.FromSeconds(10).SleepUntil(() => MethodCallCounter.AllReceivedCalls.Count() >= _maxDeliveryAttempts);

            _deadLetterMessages = await FetchAllDeadLetterMessages(bus);
        }

        [Test]
        [TestCaseSource("AllBusesTestCases")]
        public async void ThereShouldBeExactlyOneMessageOnTheDeadLetterQueue(ITestHarnessBusFactory busFactory)
        {
            await When(busFactory);

            _deadLetterMessages.Count.ShouldBe(1);
            _deadLetterMessages.Single().SomeContent.ShouldBe(_someContent);
        }

        private static async Task<List<TestCommand>> FetchAllDeadLetterMessages(IBus bus)
        {
            var deadLetterMessages = new List<TestCommand>();
            while (true)
            {
                var message = await bus.DeadLetterQueues.CommandQueue.Pop<TestCommand>();
                if (message == null) break;
                deadLetterMessages.Add(message);
            }
            return deadLetterMessages;
        }
    }
}
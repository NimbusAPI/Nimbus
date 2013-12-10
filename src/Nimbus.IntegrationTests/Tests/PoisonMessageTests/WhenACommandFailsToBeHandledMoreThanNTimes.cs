using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests
{
    [TestFixture]
    public class WhenACommandFailsToBeHandledMoreThanNTimes : SpecificationForBus
    {
        private TestCommand _testCommand;
        private string _someContent;
        private List<TestCommand> _deadLetterMessages = new List<TestCommand>();

        private const int _maxDeliveryAttempts = 7;

        public override async Task WhenAsync()
        {
            _someContent = Guid.NewGuid().ToString();
            _testCommand = new TestCommand(_someContent);

            await Subject.Send(_testCommand);
            TimeSpan.FromSeconds(10).SleepUntil(() => MessageBroker.AllReceivedCalls.Count() >= _maxDeliveryAttempts);

            _deadLetterMessages = await FetchAllDeadLetterMessages();
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
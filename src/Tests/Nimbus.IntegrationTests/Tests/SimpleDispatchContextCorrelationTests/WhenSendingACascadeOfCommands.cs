using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.Interceptors;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public class WhenSendingACascadeOfCommands : TestForBus
    {
        private const int _numExpectedMessages = 3;

        private IDispatchContext[] _dispatchContexts;
        private BrokeredMessage[] _brokeredMessages;

        protected override async Task Given()
        {
            await base.Given();
            TestInterceptor.Clear();
        }

        protected override async Task When()
        {
            var someCommand = new FirstCommand();
            await Bus.Send(someCommand);
            await TimeSpan.FromSeconds(10).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= _numExpectedMessages);

            _dispatchContexts = TestInterceptor.DispatchContexts.ToArray();
            _brokeredMessages = TestInterceptor.BrokeredMessages.ToArray();
        }

        [Test]
        public async Task TheCorrectNumberOfBrokeredMessagesShouldHaveBeenObserved()
        {
            _brokeredMessages.Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        public async Task WeShouldObserveOneDispatchContextPerMessage()
        {
            _dispatchContexts.Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        public async Task AllParentMessageIdsShouldBeDifferent()
        {
            _dispatchContexts.GroupBy(x => x.ResultOfMessageId).Count().ShouldBe(_numExpectedMessages);
        }

        [Test]
        public async Task TheCorrelationIdsShouldAllBeTheSame()
        {
            _dispatchContexts.GroupBy(x => x.CorrelationId).Count().ShouldBe(1);
        }

        [Test]
        public async Task TheThirdMessageShouldBeCausedByTheSecondMessage()
        {
            _brokeredMessages[1].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(_brokeredMessages[0].MessageId);
        }

        [Test]
        public async Task TheSecondMessageShouldBeCausedByTheFirstMessage()
        {
            _brokeredMessages[2].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(_brokeredMessages[1].MessageId);
        }

        [Test]
        public async Task TheFirstMessageShouldBeTheInitialMessage()
        {
            _brokeredMessages[0].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(null);
        }
    }
}
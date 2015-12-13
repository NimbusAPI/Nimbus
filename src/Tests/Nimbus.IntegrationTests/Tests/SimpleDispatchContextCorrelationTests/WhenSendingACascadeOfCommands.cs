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
        private NimbusMessage[] _nimbusMessages;

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
            _nimbusMessages = TestInterceptor.NimbusMessages.ToArray();
        }

        [Test]
        public async Task TheCorrectNumberOfBrokeredMessagesShouldHaveBeenObserved()
        {
            _nimbusMessages.Count().ShouldBe(_numExpectedMessages);
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
            _nimbusMessages[1].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(_nimbusMessages[0].MessageId);
        }

        [Test]
        public async Task TheSecondMessageShouldBeCausedByTheFirstMessage()
        {
            _nimbusMessages[2].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(_nimbusMessages[1].MessageId);
        }

        [Test]
        public async Task TheFirstMessageShouldBeTheInitialMessage()
        {
            _nimbusMessages[0].Properties[MessagePropertyKeys.PrecedingMessageId].ShouldBe(null);
        }
    }
}
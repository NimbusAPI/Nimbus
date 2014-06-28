using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.Interceptors;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public class WhenDispatchingACommandOnTheBusAndSendingASecondCommand : TestForBus
    {
        protected override async Task Given()
        {
            await base.Given();

            // Provide an instance of the Bus to the Handler so it can send another command without needing an IoC container
            FirstCommandHandler.Bus = Bus;
        }

        protected override async Task When()
        {
            var someCommand = new FirstCommand();
            await Bus.Send(someCommand);
            TimeSpan.FromSeconds(10).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        public async Task ThenTheCorrelationIdFromTheFirstMessageShouldByPropagatedByTheSecond()
        {
            TestInterceptor.ReceivedCorrelationIds.GroupBy(x => x).Count().ShouldBe(1);
        }
    }
}
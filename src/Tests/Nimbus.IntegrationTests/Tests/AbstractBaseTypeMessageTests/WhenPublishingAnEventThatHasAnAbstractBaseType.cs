using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.Handlers;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests
{
    public class WhenPublishingAnEventThatHasAnAbstractBaseType : TestForBus
    {
        protected override async Task When()
        {
            var busEvent = new SomeConcreteEventType();
            await Bus.Publish(busEvent);
            await TimeSpan.FromSeconds(5).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 2);
        }

        [Test]
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
		  public async Task TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeConcreteEventTypeCompetingHandler>(mb => mb.Handle(null))
                             .Count()
                             .ShouldBe(1);
        }
#pragma warning restore 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
		  
		 [Test]
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
		  public async Task TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<SomeConcreteEventTypeMulticastHandler>(mb => mb.Handle(null))
                             .Count()
                             .ShouldBe(1);
        }
#pragma warning restore 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.

		 [Test]
        public async Task TheCorrectNumberOfEventsOfThisTypeShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages
                             .OfType<SomeConcreteEventType>()
                             .Count()
                             .ShouldBe(2);
        }

        [Test]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages
                             .Count()
                             .ShouldBe(2);
        }
    }
}
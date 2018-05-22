using Nimbus.Testing.UnitTests.MessageContracts;
using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace Nimbus.Testing.UnitTests
{
    public class MessageBusStubTests
    {
        [Test]
        public async void WhenAnEventIsPublishedItShouldBeInBusEvents()
        {
            var testBus = new MessageBusStub();

            var busEvent = new WaldoEvent { Id = 55, Value = "Hello World." };

            await testBus.Publish(busEvent);

            testBus.BusEvents.Count.ShouldBe(1);
            testBus.BusEvents.Single().ShouldBeTypeOf<WaldoEvent>();

            var message = testBus.BusEvents.Single() as WaldoEvent;
            message.Id.ShouldBe(busEvent.Id);
            message.Value.ShouldBe(busEvent.Value);
        }

        [Test]
        public async void WhenMultipleEventsArePublishedTheyShouldBeInBusEventsInOrder()
        {
            var testBus = new MessageBusStub();

            var firstEvent = new WaldoEvent();
            var secondEvent = new GarplyEvent();
            var thirdEvent = new ThudEvent();

            await testBus.Publish(firstEvent);
            await testBus.Publish(secondEvent);
            await testBus.Publish(thirdEvent);

            testBus.BusEvents.Count.ShouldBe(3);
            testBus.BusEvents[0].ShouldBeTypeOf<WaldoEvent>();
            testBus.BusEvents[1].ShouldBeTypeOf<GarplyEvent>();
            testBus.BusEvents[2].ShouldBeTypeOf<ThudEvent>();
        }
    }
}
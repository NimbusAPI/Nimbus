using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.StressTests.ThreadStarvationTests.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.Handlers
{
    public class ThingAHappenedEventHandler : IHandleCompetingEvent<ThingAHappenedEvent>, IRequireBus, ILongRunningTask
    {
        public IBus Bus { get; set; }

        public async Task Handle(ThingAHappenedEvent busEvent)
        {
            for (var i = 0; i < 10; i++)
            {
                await Bus.Send(new DoThingBCommand());
            }
            await Task.Delay(TimeSpan.FromSeconds(30));
        }

        public bool IsAlive
        {
            get { return true; }
        }
    }
}
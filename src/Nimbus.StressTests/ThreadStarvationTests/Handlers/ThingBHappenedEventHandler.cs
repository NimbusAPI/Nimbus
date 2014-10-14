using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.StressTests.ThreadStarvationTests.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.Handlers
{
    public class ThingBHappenedEventHandler : IHandleCompetingEvent<ThingBHappenedEvent>, IRequireBus, ILongRunningTask
    {
        public IBus Bus { get; set; }

        public async Task Handle(ThingBHappenedEvent busEvent)
        {
            for (var i = 0; i < 10; i++)
            {
                await Bus.Send(new DoThingCCommand());
            }
            await Task.Delay(TimeSpan.FromSeconds(30));
        }

        public bool IsAlive
        {
            get { return true; }
        }
    }
}
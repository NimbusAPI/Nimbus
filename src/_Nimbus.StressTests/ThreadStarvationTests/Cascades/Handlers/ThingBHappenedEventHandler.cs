using System.Linq;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.Cascades.Handlers
{
    public class ThingBHappenedEventHandler : IHandleCompetingEvent<ThingBHappenedEvent>, IRequireBus
    {
        public const int NumberOfDoThingCCommands = 11;

        public IBus Bus { get; set; }

        public async Task Handle(ThingBHappenedEvent busEvent)
        {
            var commands = Enumerable.Range(0, NumberOfDoThingCCommands)
                                     .Select(i => new DoThingCCommand())
                                     .ToArray();

            await Bus.SendAll(commands);
        }
    }
}
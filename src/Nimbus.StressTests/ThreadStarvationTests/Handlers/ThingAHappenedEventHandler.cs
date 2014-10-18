using System.Linq;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.StressTests.ThreadStarvationTests.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.Handlers
{
    public class ThingAHappenedEventHandler : IHandleCompetingEvent<ThingAHappenedEvent>, IRequireBus, ILongRunningTask
    {
        public const int NumberOfDoThingBCommands = 11;

        public IBus Bus { get; set; }

        public async Task Handle(ThingAHappenedEvent busEvent)
        {
            var commands = Enumerable.Range(0, NumberOfDoThingBCommands)
                                     .Select(i => new DoThingBCommand())
                                     .ToArray();

            await Bus.SendAll(commands);
        }

        public bool IsAlive
        {
            get { return true; }
        }
    }
}
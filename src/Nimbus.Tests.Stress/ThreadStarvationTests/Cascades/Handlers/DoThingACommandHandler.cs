using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.Cascades.Handlers
{
    public class DoThingACommandHandler : IHandleCommand<DoThingACommand>, IRequireBus
    {
        public IBus Bus { get; set; }

        public async Task Handle(DoThingACommand busCommand)
        {
            await Bus.Publish(new ThingAHappenedEvent());
        }
    }
}
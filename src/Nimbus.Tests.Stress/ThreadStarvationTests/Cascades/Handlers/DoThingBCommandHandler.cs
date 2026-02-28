using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.Cascades.Handlers
{
    public class DoThingBCommandHandler : IHandleCommand<DoThingBCommand>, IRequireBus
    {
        public IBus Bus { get; set; }

        public async Task Handle(DoThingBCommand busCommand)
        {
            await Bus.Publish(new ThingBHappenedEvent());
        }
    }
}
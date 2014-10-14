using Nimbus.PropertyInjection;

namespace Nimbus.Infrastructure.PropertyInjection
{
    public class PropertyInjector : IPropertyInjector
    {
        public IBus Bus { get; set; }

        public void Inject(object handlerOrInterceptor)
        {
            var hasBus = handlerOrInterceptor as IRequireBus;
            if (hasBus != null)
            {
                hasBus.Bus = Bus;
            }
        }
    }
}
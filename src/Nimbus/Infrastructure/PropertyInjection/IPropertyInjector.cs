using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.PropertyInjection
{
    internal interface IPropertyInjector
    {
        void Inject(object handlerOrInterceptor, NimbusMessage nimbusMessage);
    }
}
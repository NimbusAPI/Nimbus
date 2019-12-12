using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.PropertyInjection
{
    internal interface IPropertyInjector
    {
        void Inject(object handlerOrInterceptor, NimbusMessage nimbusMessage);
    }
}
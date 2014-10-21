namespace Nimbus.Infrastructure.PropertyInjection
{
    internal interface IPropertyInjector
    {
        void Inject(object handlerOrInterceptor);
    }
}
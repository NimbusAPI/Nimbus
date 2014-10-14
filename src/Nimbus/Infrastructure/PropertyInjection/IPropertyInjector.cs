namespace Nimbus.Infrastructure.PropertyInjection
{
    public interface IPropertyInjector
    {
        void Inject(object handlerOrInterceptor);
    }
}
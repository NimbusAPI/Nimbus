using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.PropertyInjection;

namespace Nimbus.Interceptors.Outbound
{
    internal class OutboundInterceptorFactory : IOutboundInterceptorFactory
    {
        private readonly GlobalOutboundInterceptorTypesSetting _globalOutboundInterceptorTypes;
        private readonly IPropertyInjector _propertyInjector;

        public OutboundInterceptorFactory(GlobalOutboundInterceptorTypesSetting globalOutboundInterceptorTypes, IPropertyInjector propertyInjector)
        {
            _globalOutboundInterceptorTypes = globalOutboundInterceptorTypes;
            _propertyInjector = propertyInjector;
        }

        public IOutboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, NimbusMessage nimbusMessage)
        {
            return _globalOutboundInterceptorTypes
                .Value
                .Select(t => (IOutboundInterceptor) scope.Resolve(t))
                .Do(interceptor => _propertyInjector.Inject(interceptor, nimbusMessage))
                .ToArray();
        }
    }
}
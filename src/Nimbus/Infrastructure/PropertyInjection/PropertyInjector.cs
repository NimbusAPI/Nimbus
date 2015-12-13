using Nimbus.Extensions;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.PropertyInjection;

namespace Nimbus.Infrastructure.PropertyInjection
{
    internal class PropertyInjector : IPropertyInjector
    {
        private readonly IClock _clock;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILargeMessageBodyStore _largeMessageBodyStore;

        public IBus Bus { get; set; }

        public PropertyInjector(IClock clock, IDispatchContextManager dispatchContextManager, ILargeMessageBodyStore largeMessageBodyStore)
        {
            _clock = clock;
            _dispatchContextManager = dispatchContextManager;
            _largeMessageBodyStore = largeMessageBodyStore;
        }

        public void Inject(object handlerOrInterceptor, NimbusMessage nimbusMessage)
        {
            var requireBus = handlerOrInterceptor as IRequireBus;
            if (requireBus != null)
            {
                requireBus.Bus = Bus;
            }

            var requireDispatchContext = handlerOrInterceptor as IRequireDispatchContext;
            if (requireDispatchContext != null)
            {
                requireDispatchContext.DispatchContext = _dispatchContextManager.GetCurrentDispatchContext();
            }

            var requireBrokeredMessage = handlerOrInterceptor as IRequireNimbusMessage;
            if (requireBrokeredMessage != null)
            {
                requireBrokeredMessage.NimbusMessage = nimbusMessage;
            }

            var requireDateTime = handlerOrInterceptor as IRequireDateTime;
            if (requireDateTime != null)
            {
                requireDateTime.UtcNow = () => _clock.UtcNow;
            }

            var requireLargeMessageBodyStore = handlerOrInterceptor as IRequireLargeMessageBodyStore;
            if (requireLargeMessageBodyStore != null)
            {
                requireLargeMessageBodyStore.LargeMessageBodyStore = _largeMessageBodyStore;
            }

            var requireMessageProperties = handlerOrInterceptor as IRequireMessageProperties;
            if (requireMessageProperties != null)
            {
                var properties = nimbusMessage.ExtractProperties();
                requireMessageProperties.MessageProperties = properties;
            }
        }
    }
}
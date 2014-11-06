using System.Linq;
using Microsoft.ServiceBus.Messaging;
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

        public void Inject(object handlerOrInterceptor, BrokeredMessage brokeredMessage)
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

            var requireBrokeredMessage = handlerOrInterceptor as IRequireBrokeredMessage;
            if (requireBrokeredMessage != null)
            {
                requireBrokeredMessage.BrokeredMessage = brokeredMessage;
            }

            var requireClock = handlerOrInterceptor as IRequireClock;
            if (requireClock != null)
            {
                requireClock.Clock = _clock;
            }

            var requireLargeMessageBodyStore = handlerOrInterceptor as IRequireLargeMessageBodyStore;
            if (requireLargeMessageBodyStore != null)
            {
                requireLargeMessageBodyStore.LargeMessageBodyStore = _largeMessageBodyStore;
            }

            var requireMessageProperties = handlerOrInterceptor as IRequireMessageProperties;
            if (requireMessageProperties != null)
            {
                var properties = brokeredMessage.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                properties[MessagePropertyKeys.MessageId] = brokeredMessage.MessageId;
                properties[MessagePropertyKeys.CorrelationId] = brokeredMessage.CorrelationId;

                requireMessageProperties.MessageProperties = properties;
            }
        }
    }
}
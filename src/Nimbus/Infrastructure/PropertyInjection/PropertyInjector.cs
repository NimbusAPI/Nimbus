using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.PropertyInjection;

namespace Nimbus.Infrastructure.PropertyInjection
{
    internal class PropertyInjector : IPropertyInjector
    {
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly IClock _clock;

        public IBus Bus { get; set; }

        public PropertyInjector(IDispatchContextManager dispatchContextManager, IClock clock)
        {
            _dispatchContextManager = dispatchContextManager;
            _clock = clock;
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
        }
    }
}
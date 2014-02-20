using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    public class DefaultMessageHandlerFactory : ICommandHandlerFactory, IRequestBroker, IMulticastRequestBroker, IMulticastEventBroker, ICompetingEventBroker
    {
        private readonly ITypeProvider _typeProvider;

        public DefaultMessageHandlerFactory(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        // ReSharper disable ConvertClosureToMethodGroup

        public OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>() where TBusCommand : IBusCommand
        {
            var handler = _typeProvider.CommandHandlerTypes
                          .Where(t => typeof(IHandleCommand<TBusCommand>).IsAssignableFrom(t))
                          .Select(type => CreateInstance<IHandleCommand<TBusCommand>>(type))
                          .First();

            return new OwnedComponent<IHandleCommand<TBusCommand>>(handler, handler as IDisposable);
        }

        public virtual TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            return _typeProvider.RequestHandlerTypes
                                .Where(t => typeof (IHandleRequest<TBusRequest, TBusResponse>).IsAssignableFrom(t))
                                .Select(type => CreateInstance<IHandleRequest<TBusRequest, TBusResponse>>(type))
                                .First()
                                .Handle(request).Result;
        }

        public virtual IEnumerable<TBusResponse> HandleMulticast<TBusRequest, TBusResponse>(TBusRequest request, TimeSpan timeout)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            return _typeProvider.RequestHandlerTypes
                                .Where(t => typeof (IHandleRequest<TBusRequest, TBusResponse>).IsAssignableFrom(t))
                                .Select(type => CreateInstance<IHandleRequest<TBusRequest, TBusResponse>>(type))
                                .Select(handler => Task.Run(() => handler.Handle(request)))
                                .ReturnOpportunistically(timeout);
        }

        public virtual void PublishMulticast<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            _typeProvider.MulticastEventHandlerTypes
                         .Where(t => typeof (IHandleMulticastEvent<TBusEvent>).IsAssignableFrom(t))
                         .Select(type => CreateInstance<IHandleMulticastEvent<TBusEvent>>(type))
                         .Do(handler => handler.Handle(busEvent))
                         .Done();
        }

        public virtual void PublishCompeting<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            _typeProvider.CompetingEventHandlerTypes
                         .Where(t => typeof (IHandleCompetingEvent<TBusEvent>).IsAssignableFrom(t))
                         .Select(type => CreateInstance<IHandleCompetingEvent<TBusEvent>>(type))
                         .Do(handler => handler.Handle(busEvent))
                         .Done();
        }

        // ReSharper restore ConvertClosureToMethodGroup

        private THandler CreateInstance<THandler>(Type handlerType)
        {
            try
            {
                var result = (THandler) Activator.CreateInstance(handlerType);
                return result;
            }
            catch (Exception exc)
            {
                var message = (
                                  "The {0} can only broker messages to handlers that have default constructors (i.e. ones with no parameters). " +
                                  "If you'd like to use constructor injection on your handlers, have a look at the examples provided in the README " +
                                  "about how to wire things up via an IoC container."
                              ).FormatWith(GetType().Name);

                throw new BusException(message, exc);
            }
        }
    }
}
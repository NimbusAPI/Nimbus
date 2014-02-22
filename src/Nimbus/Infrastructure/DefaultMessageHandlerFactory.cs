using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    public sealed class DefaultMessageHandlerFactory : ICommandHandlerFactory,
                                                       IRequestHandlerFactory,
                                                       IMulticastRequestHandlerFactory,
                                                       IMulticastEventHandlerFactory,
                                                       ICompetingEventHandlerFactory
    {
        private readonly ITypeProvider _typeProvider;

        public DefaultMessageHandlerFactory(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        // ReSharper disable ConvertClosureToMethodGroup

        OwnedComponent<IHandleCommand<TBusCommand>> ICommandHandlerFactory.GetHandler<TBusCommand>()
        {
            var handler = _typeProvider.CommandHandlerTypes
                                       .Where(t => typeof (IHandleCommand<TBusCommand>).IsAssignableFrom(t))
                                       .Select(type => CreateInstance<IHandleCommand<TBusCommand>>(type))
                                       .First();

            return new OwnedComponent<IHandleCommand<TBusCommand>>(handler, handler as IDisposable);
        }

        OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>> IRequestHandlerFactory.GetHandler<TBusRequest, TBusResponse>()

        {
            var handler = _typeProvider.RequestHandlerTypes
                                       .Where(t => typeof (IHandleRequest<TBusRequest, TBusResponse>).IsAssignableFrom(t))
                                       .Select(type => CreateInstance<IHandleRequest<TBusRequest, TBusResponse>>(type))
                                       .First();

            return new OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>>(handler, handler as IDisposable);
        }

        OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>> IMulticastRequestHandlerFactory.GetHandlers<TBusRequest, TBusResponse>()
        {
            var handlers = _typeProvider.RequestHandlerTypes
                                        .Where(t => typeof (IHandleRequest<TBusRequest, TBusResponse>).IsAssignableFrom(t))
                                        .Select(type => CreateInstance<IHandleRequest<TBusRequest, TBusResponse>>(type))
                                        .ToArray();
            var wrapper = new DisposableWrapper(handlers.Cast<object>().ToArray());

            return new OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>(handlers, wrapper);
        }

        OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>> IMulticastEventHandlerFactory.GetHandlers<TBusEvent>()
        {
            var handlers = _typeProvider.MulticastEventHandlerTypes
                                        .Where(t => typeof (IHandleMulticastEvent<TBusEvent>).IsAssignableFrom(t))
                                        .Select(type => CreateInstance<IHandleMulticastEvent<TBusEvent>>(type))
                                        .ToArray();
            var wrapper = new DisposableWrapper(handlers.Cast<object>().ToArray());

            return new OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>>(handlers, wrapper);
        }

        OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> ICompetingEventHandlerFactory.GetHandlers<TBusEvent>()
        {
            var handlers = _typeProvider.CompetingEventHandlerTypes
                                        .Where(t => typeof (IHandleCompetingEvent<TBusEvent>).IsAssignableFrom(t))
                                        .Select(type => CreateInstance<IHandleCompetingEvent<TBusEvent>>(type))
                                        .ToArray();
            var wrapper = new DisposableWrapper(handlers.Cast<object>().ToArray());

            return new OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>>(handlers, wrapper);
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

        /// <summary>
        ///     Used to encapsulate a bunch of IDisposable objects inside a single wrapper so that we can dispose of them cleanly
        ///     at the end of a message-handling unit of work.
        /// </summary>
        internal class DisposableWrapper : IDisposable
        {
            private readonly object[] _components;

            public DisposableWrapper(params object[] components)
            {
                _components = components;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposing) return;

                foreach (var component in _components.OfType<IDisposable>())
                {
                    try
                    {
                        component.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
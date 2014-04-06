using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.ThroughputTests.EventHandlers;
using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests.Infrastructure
{
    public class FakeHandlerFactory : ICommandHandlerFactory, IMulticastEventHandlerFactory, ICompetingEventHandlerFactory, IRequestHandlerFactory, IMulticastRequestHandlerFactory
    {
        private readonly FakeHandler _fakeHandler;

        public FakeHandlerFactory(FakeHandler fakeHandler)
        {
            _fakeHandler = fakeHandler;
        }

      
        public OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>() where TBusCommand : IBusCommand
        {
            return new OwnedComponent<IHandleCommand<TBusCommand>>((IHandleCommand<TBusCommand>) _fakeHandler);
        }

        OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> ICompetingEventHandlerFactory.GetHandlers<TBusEvent>()
        {
            var handlers = new[] {_fakeHandler}.Cast<IHandleCompetingEvent<TBusEvent>>();
            return new OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>>(handlers);
        }

        public OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent
        {
            var handlers = new[] {_fakeHandler}.Cast<IHandleMulticastEvent<TBusEvent>>();
            return new OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>>(handlers);
        }

        public OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>> GetHandlers<TBusRequest, TBusResponse>()
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
        {
            var handlers = new[] {_fakeHandler}.Cast<IHandleRequest<TBusRequest, TBusResponse>>();
            return new OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>(handlers);
        }

        public OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>> GetHandler<TBusRequest, TBusResponse>() where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            return new OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>>((IHandleRequest<TBusRequest, TBusResponse>) _fakeHandler);
        }
    }
}
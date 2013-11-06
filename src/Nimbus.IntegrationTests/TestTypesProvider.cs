using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests
{
    public class TestTypesProvider : ITypeProvider
    {
        private readonly Type[] _commandHandlerTypes;
        private readonly Type[] _timeoutHandlerTypes;
        private readonly Type[] _eventHandlerTypes;
        private readonly Type[] _requestHandlerTypes;
        private readonly Type[] _commandTypes;
        private readonly Type[] _timeoutTypes;
        private readonly Type[] _eventTypes;
        private readonly Type[] _requestTypes;

        public TestTypesProvider(IEnumerable<Type> commandHandlerTypes,
                                 IEnumerable<Type> commandTypes,
                                 IEnumerable<Type> timeoutHandlerTypes,
                                 IEnumerable<Type> timeoutTypes,
                                 IEnumerable<Type> requestHandlerTypes,
                                 IEnumerable<Type> requestTypes,
                                 IEnumerable<Type> eventHandlerTypes,
                                 IEnumerable<Type> eventTypes)
        {
            _commandHandlerTypes = commandHandlerTypes.ToArray();
            _commandTypes = commandTypes.ToArray();
            _timeoutHandlerTypes = timeoutHandlerTypes.ToArray();
            _timeoutTypes = timeoutTypes.ToArray();
            _requestHandlerTypes = requestHandlerTypes.ToArray();
            _requestTypes = requestTypes.ToArray();
            _eventHandlerTypes = eventHandlerTypes.ToArray();
            _eventTypes = eventTypes.ToArray();

            _commandHandlerTypes.All(t => t.IsClosedTypeOf(typeof (IHandleCommand<>))).ShouldBe(true);
            _commandTypes.All(t => typeof (IBusCommand).IsAssignableFrom(t)).ShouldBe(true);

            _timeoutHandlerTypes.All(t => t.IsClosedTypeOf(typeof(IHandleTimeout<>))).ShouldBe(true);
            _timeoutTypes.All(t => typeof(IBusTimeout).IsAssignableFrom(t)).ShouldBe(true);

            _requestHandlerTypes.All(t => t.IsClosedTypeOf(typeof (IHandleRequest<,>))).ShouldBe(true);
            _requestTypes.All(t => typeof (IBusRequest).IsAssignableFrom(t)).ShouldBe(true);

            _eventHandlerTypes.All(t => t.IsClosedTypeOf(typeof (IHandleEvent<>))).ShouldBe(true);
            _eventTypes.All(t => typeof (IBusEvent).IsAssignableFrom(t)).ShouldBe(true);
        }

        public IEnumerable<Type> CommandHandlerTypes
        {
            get { return _commandHandlerTypes; }
        }

        public IEnumerable<Type> CommandTypes
        {
            get { return _commandTypes; }
        }

        public IEnumerable<Type> TimeoutHandlerTypes
        {
            get { return _timeoutHandlerTypes; }
        }

        public IEnumerable<Type> TimeoutTypes
        {
            get { return _timeoutTypes; }
        }

        public IEnumerable<Type> EventHandlerTypes
        {
            get { return _eventHandlerTypes; }
        }

        public IEnumerable<Type> EventTypes
        {
            get { return _eventTypes; }
        }

        public IEnumerable<Type> RequestHandlerTypes
        {
            get { return _requestHandlerTypes; }
        }

        public IEnumerable<Type> RequestTypes
        {
            get { return _requestTypes; }
        }
    }
}